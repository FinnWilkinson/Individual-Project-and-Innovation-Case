using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

#if ENABLE_WINMD_SUPPORT
using Windows.UI.Xaml;
using Windows.Graphics.Imaging;

// Include winrt components
using HoloLensForCV;
#endif

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using System.Threading;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnityExample;



// App permissions, modify the appx file for research mode streams
// https://docs.microsoft.com/en-us/windows/uwp/packaging/app-capability-declarations

// Reimplement as list loop structure... 
namespace HoloLensPoolAid
{
    // Using the hololens for cv .winmd file for runtime support
    // Build HoloLensForCV c++ project (x86) and copy all output files
    // to Assets->Plugins->x86
    // https://docs.unity3d.com/2018.4/Documentation/Manual/IL2CPP-WindowsRuntimeSupport.html
    public class SensorStreams : MonoBehaviour
    {
        public Text myText;

        private float interval = (1 / 20);
        private float nextTime = 0;

        public enum SensorTypeUnity
        {
            Undefined = -1,
            PhotoVideo = 0
        }
        /// <summary>
        /// variable for Photo Video sensor
        /// </summary>
        public SensorTypeUnity sensorTypePv;

        /// <summary>
        /// GameObject to show video streams.
        /// </summary>
        public GameObject cameraFrameGO;

        /// <summary>
        /// Cached materials for applying to game objects.
        /// </summary>
        private Material cameraFrameMaterial;

        /// </summary>
        /// Textures created from input byte arrays.
        /// </summary>
        // PV
        private Texture2D cameraFrameTexture;

        private bool _mediaFrameSourceGroupsStarted = false;

#if ENABLE_WINMD_SUPPORT
        // Enable winmd support to include winmd files. Will not
        // run in Unity editor.
        private SensorFrameStreamer _sensorFrameStreamerPv;
        private SpatialPerception _spatialPerception;

        /// <summary>
        /// Media frame source groups for sensor stream.
        /// </summary>
        private MediaFrameSourceGroup _pvMediaFrameSourceGroup;

        SensorType _sensorType;
#endif

        // Gesture handler
        GestureRecognizer _gestureRecognizer;

        #region UnityMethods

        // Use this for initialization
        async void Start()
        {
            // Initialize gesture handler
            InitializeHandler();

            // Get the material components from quad game objects.
            cameraFrameMaterial = cameraFrameGO.GetComponent<MeshRenderer>().material;

            // Start the media frame source groups.
            await StartHoloLensMediaFrameSourceGroups();

        }

        // Update is called once per frame
        void Update()
        {
            if(Time.time >= nextTime)
            {
                // Get next camera frame
                UpdateHoloLensMediaFrameSourceGroup();

                // Do every interval seconds
                nextTime += interval;
            }



        }

        async void OnApplicationQuit()
        {
            await StopHoloLensMediaFrameSourceGroup();
        }

        #endregion
        async Task StartHoloLensMediaFrameSourceGroups()
        {
#if ENABLE_WINMD_SUPPORT
            // Plugin doesn't work in the Unity editor
            myText.text = "Initalizing MediaFrameSourceGroups...";

            // Photo Video sensor stream
            _sensorFrameStreamerPv = new SensorFrameStreamer();
            _sensorType = (SensorType)sensorTypePv;
            _sensorFrameStreamerPv.Enable(_sensorType);

            // Spatial perception initialisation
            _spatialPerception = new SpatialPerception();

            // Enable Photo Video media frame source group
            _pvMediaFrameSourceGroup = new MediaFrameSourceGroup(
                MediaFrameSourceGroupType.PhotoVideoCamera,
                _spatialPerception,
                _sensorFrameStreamerPv);
            _pvMediaFrameSourceGroup.Enable(_sensorType);

            // Start media frame source group
            myText.text = "Starting MediaFrameSourceGroups...";

            await _pvMediaFrameSourceGroup.StartAsync();

            _mediaFrameSourceGroupsStarted = true;

            myText.text = "MediaFrameSourceGroups started...";
#endif
        }

        // Get the latest frame from hololens media
        // frame source group -- not needed
        unsafe void UpdateHoloLensMediaFrameSourceGroup()
        {
#if ENABLE_WINMD_SUPPORT
            if (!_mediaFrameSourceGroupsStarted ||
                _pvMediaFrameSourceGroup == null)
            {
                return;
            }

            // Get latest Photo video frames
            SensorFrame latestPvCameraFrame =
                _pvMediaFrameSourceGroup.GetLatestSensorFrame(
                _sensorType);

            // Convert the frame to be unity viewable
            var pvFrame = SoftwareBitmap.Convert(
               latestPvCameraFrame.SoftwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore);

            // Display the incoming pv frame as a texture.
            // Set texture to the desired renderer
            Destroy(cameraFrameTexture);
            cameraFrameTexture = new Texture2D(
                pvFrame.PixelWidth,
                pvFrame.PixelHeight,
                TextureFormat.BGRA32, false);

            // Get byte array, update unity material with texture (RGBA)
            byte* inBytesPV = GetByteArrayFromSoftwareBitmap(pvFrame);
            cameraFrameTexture.LoadRawTextureData((IntPtr)inBytesPV, pvFrame.PixelWidth * pvFrame.PixelHeight * 4);
            cameraFrameTexture.Apply();



            // Aruco detection
            //arucoDetection();
            arucoDetection();
         
            //update material texture
            cameraFrameMaterial.mainTexture = cameraFrameTexture;

            //myText.text = "Began streaming sensor frames. Double tap to end streaming.";
#endif
        }


        /// <summary>
        /// Stop the media frame source groups.
        /// </summary>
        /// <returns></returns>
        async Task StopHoloLensMediaFrameSourceGroup()
        {
#if ENABLE_WINMD_SUPPORT
            if (!_mediaFrameSourceGroupsStarted ||
                _pvMediaFrameSourceGroup == null)
            {
                return;
            }

            // Wait for frame source groups to stop.
            await _pvMediaFrameSourceGroup.StopAsync();
            _pvMediaFrameSourceGroup = null;

            // Set to null value
            _sensorFrameStreamerPv = null;

            // Bool to indicate closing
            _mediaFrameSourceGroupsStarted = false;

            myText.text = "Stopped streaming sensor frames. Okay to exit app.";
#endif
        }

        #region ComImport
        // https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }
        #endregion

#if ENABLE_WINMD_SUPPORT
        // Get byte array from software bitmap.
        // https://github.com/qian256/HoloLensARToolKit/blob/master/ARToolKitUWP-Unity/Scripts/ARUWPVideo.cs
        unsafe byte* GetByteArrayFromSoftwareBitmap(SoftwareBitmap sb)
        {
            if (sb == null) return null;

            SoftwareBitmap sbCopy = new SoftwareBitmap(sb.BitmapPixelFormat, sb.PixelWidth, sb.PixelHeight);
            Interlocked.Exchange(ref sbCopy, sb);
            using (var input = sbCopy.LockBuffer(BitmapBufferAccessMode.Read))
            using (var inputReference = input.CreateReference())
            {
                byte* inputBytes;
                uint inputCapacity;
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out inputBytes, out inputCapacity);
                return inputBytes;
            }
        }
#endif

        #region TapGestureHandler
        private void InitializeHandler()
        {
            // New recognizer class
            _gestureRecognizer = new GestureRecognizer();

            // Set tap as a recognizable gesture
            _gestureRecognizer.SetRecognizableGestures(GestureSettings.DoubleTap);

            // Begin listening for gestures
            _gestureRecognizer.StartCapturingGestures();

            // Capture on gesture events with delegate handler
            _gestureRecognizer.Tapped += GestureRecognizer_Tapped;

            Debug.Log("Gesture recognizer initialized.");
        }

        // On tapped event, stop all frame source groups
        private void GestureRecognizer_Tapped(TappedEventArgs obj)
        {
            StopHoloLensMediaFrameSourceGroup();
            CloseHandler();
        }

        private void CloseHandler()
        {
            _gestureRecognizer.StopCapturingGestures();
            _gestureRecognizer.Dispose();
        }
        #endregion


        // Aruco detection and setup
        public GameObject ARobject;

        private bool showRejectedCroners = false;
        private bool applyEstPose = true;
        private float markerLength = 0.1f;

        
         
        unsafe private void arucoDetection()
        {
            Mat rgbMat = new Mat(cameraFrameTexture.height, cameraFrameTexture.width, CvType.CV_8UC3);

            Utils.texture2DToMat(cameraFrameTexture, rgbMat, flip:false);

            Mat ids = new Mat();
            List<Mat> corners = new List<Mat>();
            List<Mat> rejectedCorners = new List<Mat>();
            DetectorParameters parameters = DetectorParameters.create();

            // Camera calib stuff
            /*
            Mat distCoeffs = new MatOfDouble(-0.002602963842533594, -0.008751170499511022, -0.0022398259556777236, -5.941804169976817e-05, 0.0);
            */

            Aruco.detectMarkers(rgbMat, Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50), corners, ids, parameters, rejectedCorners);

            myText.text = "Detected " + ids.total().ToString() + " markers. Rejected " + rejectedCorners.Count.ToString() + " corners." ;

            if(ids.total() > 0)
            {
                // Draw markers 
                Aruco.drawDetectedMarkers(rgbMat, corners, ids);

                // Camera calibration
                float width = rgbMat.width();
                float height = rgbMat.height();

                float imageSizeScale = 1.0f;

                int max_d = (int)Mathf.Max(width, height);
                double fx = max_d;
                double fy = max_d;
                double cx = width / 2.0f;
                double cy = height / 2.0f;
                Mat cameraMatrix = new Mat(3, 3, CvType.CV_64FC1);
                cameraMatrix.put(0, 0, 677.8968352717175f);
                cameraMatrix.put(0, 1, 0);
                cameraMatrix.put(0, 2, 439.2388714449508f);
                cameraMatrix.put(1, 0, 0);
                cameraMatrix.put(1, 1, 677.1775976226464f);
                cameraMatrix.put(1, 2, 231.50848952714483f);
                cameraMatrix.put(2, 0, 0);
                cameraMatrix.put(2, 1, 0);
                cameraMatrix.put(2, 2, 1.0f);

                MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);

                Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
                double apertureWidth = 0;
                double apertureHeight = 0;
                double[] fovx = new double[1];
                double[] fovy = new double[1];
                double[] focalLength = new double[1];
                Point principalPoint = new Point(0, 0);
                double[] aspectratio = new double[1];

                Calib3d.calibrationMatrixValues(cameraMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);


                // Get pose transformations and draw axis 
                Mat rvecs = new Mat();
                Mat tvecs = new Mat();
                Aruco.estimatePoseSingleMarkers(corners, 0.1f, cameraMatrix, distCoeffs, rvecs, tvecs);
                Calib3d.drawFrameAxes(rgbMat, cameraMatrix, distCoeffs, rvecs, tvecs, 0.05f);




                // Put cube in place
                ARobject.SetActive(true);

                // Get translation vector
                double[] tvecArr = tvecs.get(0, 0);
                // Get rotation vector
                double[] rvecArr = rvecs.get(0, 0);

                // Convert rotation vectyor to matric
                Mat rvec = new Mat(3, 1, CvType.CV_64FC1);
                rvec.put(0, 0, rvecArr);

                Mat rotMat = new Mat(3, 3, CvType.CV_64FC1);
                double[] rotMatArr = new double[rotMat.total()];
                Calib3d.Rodrigues(rvec, rotMat);
                rotMat.get(0, 0, rotMatArr);

                // Convert OpenCV camera extrinsic parameters to Unity Matrix4x4.
                Matrix4x4 transformationM = new Matrix4x4(); // from OpenCV
                transformationM.SetRow(0, new Vector4((float)rotMatArr[0], (float)rotMatArr[1], (float)rotMatArr[2], (float)tvecArr[0]));
                transformationM.SetRow(1, new Vector4((float)rotMatArr[3], (float)rotMatArr[4], (float)rotMatArr[5], (float)tvecArr[1]));
                transformationM.SetRow(2, new Vector4((float)rotMatArr[6], (float)rotMatArr[7], (float)rotMatArr[8], (float)tvecArr[2]));
                transformationM.SetRow(3, new Vector4(0, 0, 0, 1));

                Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
                // right-handed coordinates system (OpenCV) to left-handed one (Unity)
                // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                Matrix4x4 ARM = invertYM * transformationM * invertYM;

                ARM = Camera.main.transform.localToWorldMatrix * ARM;

                ARUtils.SetTransformFromMatrix(ARobject.transform, ref ARM);
                ARobject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);


                // Update material texture
                Utils.matToTexture2D(rgbMat, cameraFrameTexture, flip:false);
                cameraFrameTexture.Apply();
            }

        }


        
    }
}



