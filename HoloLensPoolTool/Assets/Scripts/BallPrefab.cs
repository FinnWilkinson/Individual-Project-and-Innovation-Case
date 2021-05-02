using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class BallPrefab : MonoBehaviour
{
    public GameObject ball;
    public GameObject cueMarker;
    public GameObject buttonsHolder;

    private GameObject mainCamera;
    private GameObject removeBtn;
    private GameObject makeCueBallBtn;

    public GameObject minusBtn;
    public GameObject plusBtn;
    public GameObject degreeBtnHolder;

    public Material ballMat;
    public Material cueBallMat;

    public Material cueMarkerDefault;
    public Material cueMarkerHovered;

    public bool isCueBall;
    public bool selected;
    public bool allowUI;

    // Start is called before the first frame update
    void Start()
    {
        //Can scale by tables x scale, as only allowed proportional scale, so all axis scale by same amount
        float tableScale = GameObject.Find("Pool_Table").gameObject.transform.Find("Table").transform.localScale.x;
        this.transform.localScale = this.transform.localScale * tableScale;

        //get main camera object
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        //set values
        isCueBall = false;
        selected = false;
        allowUI = !GameObject.Find("Toggle_Ball_UI_BTN").GetComponent<Interactable>().IsToggled;
        cueMarker.SetActive(false);
        buttonsHolder.SetActive(false);
        degreeBtnHolder.SetActive(false);

        //get button game objects 
        removeBtn = buttonsHolder.transform.GetChild(0).gameObject;
        makeCueBallBtn = buttonsHolder.transform.GetChild(1).gameObject;

        //Adding button listeners for ball UI
        removeBtn.GetComponent<Interactable>().OnClick.AddListener(() => removeThisBall());
        makeCueBallBtn.GetComponent<Interactable>().OnClick.AddListener(() => setCueBall());
        plusBtn.GetComponent<Interactable>().OnClick.AddListener(() => addDegree());
        minusBtn.GetComponent<Interactable>().OnClick.AddListener(() => minusDegree());

        //Add listeners so know when marker is being manipulated.
        cueMarker.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((data) => cueMarkerManipulationStarted(data));
        cueMarker.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener((data) => cueMarkerManipulationEnded(data));

        //Add Listener so know when ball is being manipulated.
        ball.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((data) => ballManipulationStarted(data));

        //Add listener so know when hover entered and exited for cue marker
        cueMarker.GetComponent<ObjectManipulator>().OnHoverEntered.AddListener((data) => markerHoverEntered(data));
        cueMarker.GetComponent<ObjectManipulator>().OnHoverExited.AddListener((data) => markerHoverExit(data));
    }

    // Update is called once per frame
    void Update()
    {
        //keep buttons with ball as it is moved.
        if (selected)
        {
            buttonsHolder.transform.position = ball.transform.position + new Vector3(0.0f, 2.0f * this.transform.localScale.y, 0.0f);
            buttonsHolder.transform.LookAt(mainCamera.transform);
        }
        if (isCueBall && allowUI)
        {
            degreeBtnHolder.transform.position = ball.transform.position;
            degreeBtnHolder.transform.LookAt(cueMarker.transform);
        }
    }

    //When user grabs marker, freeze cueball position and change hinge anchor, makes kinematic so not bound to hinge spring and marker moves with ball.
    ManipulationEventData cueMarkerManipulationStarted(ManipulationEventData data)
    {
        cueMarker.GetComponent<Rigidbody>().isKinematic = true;
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        cueMarker.GetComponent<HingeJoint>().connectedAnchor = ball.transform.position;
        return data;
    }

    //Once marker is let go, allow for ball to be moved again
    ManipulationEventData cueMarkerManipulationEnded(ManipulationEventData data)
    {
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        return data;
    }

    // When ball grabbed, make marker kinematic so spring hinge doesnt effect it, and marker moves with the ball
    ManipulationEventData ballManipulationStarted(ManipulationEventData data)
    {
        cueMarker.GetComponent<Rigidbody>().isKinematic = true;
        return data;
    }

    // When cue marker is hovered over, change material so know youre looking at it
    ManipulationEventData markerHoverEntered(ManipulationEventData data)
    {
        cueMarker.GetComponent<MeshRenderer>().material = cueMarkerHovered;
        return data;
    }

    // When cue marker is not hovered over anymore, change material back to default
    ManipulationEventData markerHoverExit(ManipulationEventData data)
    {
        cueMarker.GetComponent<MeshRenderer>().material = cueMarkerDefault;
        return data;
    }

    //Makes this ball cue ball, and all others not
    void setCueBall()
    {
        foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
        {
            ballObj.GetComponent<BallPrefab>().isCueBall = false;
            ballObj.GetComponent<BallPrefab>().cueMarker.SetActive(false);
            ballObj.GetComponent<BallPrefab>().degreeBtnHolder.SetActive(false);
            ballObj.GetComponent<BallPrefab>().ball.GetComponent<MeshRenderer>().material = ballMat;
            ballObj.GetComponent<BallPrefab>().ball.gameObject.layer = 10; //Layer 10 = Ball
        }
        this.cueMarker.SetActive(true);
        this.isCueBall = true;
        ball.GetComponent<MeshRenderer>().material = cueBallMat;
        ball.layer = 11; //Layer 11 = Cue_Ball
        degreeBtnHolder.SetActive(true);
    }

    // Shows this balls UI, Invoked by ballProperties.cs when hovered over by user
    // Makes cue marker kinematic so not bound to hinge spring and marker moves with ball.
    public void SetSelected(bool val)
    {
        if (allowUI)
        {
            selected = val;
            buttonsHolder.SetActive(val);
        }
    }

    //Removes this object from game scene
    void removeThisBall()
    {
        Destroy(this.gameObject);
    }

    // Rotates cue marker clockwise 1 degree
    void addDegree()
    {
        cueMarker.GetComponent<Rigidbody>().isKinematic = false;
        cueMarker.GetComponent<Rigidbody>().WakeUp();

        cueMarker.GetComponent<HingeJoint>().connectedAnchor = ball.transform.position;

        HingeJoint hinge = cueMarker.GetComponent<HingeJoint>();
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.spring = 1000;
        hingeSpring.damper = 0;
        hingeSpring.targetPosition = hinge.angle + 1.0f;
        hinge.spring = hingeSpring;
        hinge.useSpring = true;
    }

    // Rotates cue marker anti-clockwise 1 degree
    void minusDegree()
    {
        cueMarker.GetComponent<Rigidbody>().isKinematic = false;
        cueMarker.GetComponent<Rigidbody>().WakeUp();

        cueMarker.GetComponent<HingeJoint>().connectedAnchor = ball.transform.position;

        HingeJoint hinge = cueMarker.GetComponent<HingeJoint>();
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.spring = 1000;
        hingeSpring.damper = 0;
        hingeSpring.targetPosition = hinge.angle - 1.0f;
        hinge.spring = hingeSpring;
        hinge.useSpring = true;
    }
}
