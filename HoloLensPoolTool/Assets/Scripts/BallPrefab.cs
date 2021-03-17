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

    public Material ballMat;
    public Material cueBallMat;

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

        //get button game objects 
        removeBtn = buttonsHolder.transform.GetChild(0).gameObject;
        makeCueBallBtn = buttonsHolder.transform.GetChild(1).gameObject;

        //Adding button listeners for ball UI
        removeBtn.GetComponent<Interactable>().OnClick.AddListener(() => removeThisBall());
        makeCueBallBtn.GetComponent<Interactable>().OnClick.AddListener(() => setCueBall());

        //Add listeners so know when ball is being moved.
        ball.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((data) => ballManipulationStarted(data));
        ball.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener((data) => ballManipulationEnded(data));

        //Add listeners so know when marker is being manipulated.
        cueMarker.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((data) => cueMarkerManipulationStarted(data));
        cueMarker.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener((data) => cueMarkerManipulationEnded(data));
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
    }

    //When user grabs ball, turn off aim guide  NOT IN USE ATM - USE IF BAD HOLOLENS PERFORMANCE
    ManipulationEventData ballManipulationStarted(ManipulationEventData data)
    {
        if (isCueBall)
        {
            //cueMarker.GetComponent<LineRenderer>().enabled = false;
        }
        return data;
    }

    //Once ball is let go, turn on aim guide    NOT IN USE ATM - USE IF BAD HOLOLENS PERFORMANCE
    ManipulationEventData ballManipulationEnded(ManipulationEventData data)
    {
        if (isCueBall)
        {
            //cueMarker.GetComponent<LineRenderer>().enabled = true;
        }
        return data;
    }

    //When user grabs marker, freeze cueball position and change hinge anchor
    ManipulationEventData cueMarkerManipulationStarted(ManipulationEventData data)
    {
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        //cueMarker.GetComponent<HingeJoint>().anchor = cueMarker.transform.position + new Vector3(0.0f, 8.675f, 0.0f);
        cueMarker.GetComponent<HingeJoint>().connectedAnchor = ball.transform.position;
        return data;
    }

    //Once marker is let go, allow for ball to be moved again
    ManipulationEventData cueMarkerManipulationEnded(ManipulationEventData data)
    {
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        return data;
    }

    //Makes this ball cue ball, and all others not
    void setCueBall()
    {
        foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
        {
            ballObj.GetComponent<BallPrefab>().isCueBall = false;
            ballObj.GetComponent<BallPrefab>().cueMarker.SetActive(false);
            ballObj.GetComponent<BallPrefab>().ball.GetComponent<MeshRenderer>().material = ballMat;
        }
        this.cueMarker.SetActive(true);
        this.isCueBall = true;
        ball.GetComponent<MeshRenderer>().material = cueBallMat;
    }

    //Shows this balls UI, Invoked by ballProperties.cs when hovered over by user
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
}
