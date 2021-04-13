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

        
         
        private void arucoDetection()
        {
            Mat rgbMat = new Mat(cameraFrameTexture.height, cameraFrameTexture.width, CvType.CV_8UC3);

            Utils.fastTexture2DToMat(cameraFrameTexture, rgbMat);
            Mat ids = new Mat();
            List<Mat> corners = new List<Mat>();
            List<Mat> rejectedCorners = new List<Mat>();
            DetectorParameters parameters = DetectorParameters.create();


            Aruco.detectMarkers(rgbMat, Aruco.getPredefinedDictionary(Aruco.DICT_6X6_250), corners, ids, parameters, rejectedCorners);

            myText.text = "Detected " + ids.total().ToString() + " markers. Rejected " + rejectedCorners.Count.ToString() + " corners." ;

            if(ids.total() > 0)
            {
                Aruco.drawDetectedMarkers(rgbMat, corners, ids);

                Utils.matToTexture2D(rgbMat, cameraFrameTexture);

                //update material texture
                cameraFrameMaterial.mainTexture = cameraFrameTexture;

                StopHoloLensMediaFrameSourceGroup();
            }
            if(rejectedCorners.Count > 0)
            {
                Aruco.drawDetectedMarkers(rgbMat, rejectedCorners);
            }
        }
    }
}



