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

    public bool isCueBall;
    public bool selected;
    public bool allowUI;

    private bool moving;

    // Start is called before the first frame update
    void Start()
    {
        //get main camera object
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        //set values
        isCueBall = false;
        selected = false;
        moving = false;
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
        //make marker follow cueball if moved
        if (moving)
        {
            cueMarker.transform.position = ball.transform.position + new Vector3(0.0f, 0.0f, -0.65f * this.transform.localScale.z);
        }
    }

    //When user grabs ball, make marker follow (and turn off aim guide)
    ManipulationEventData ballManipulationStarted(ManipulationEventData data)
    {
        cueMarker.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
        moving = true;
        return data;
    }

    //Once ball is let go, dont lock marker position (turn on aim guide)
    ManipulationEventData ballManipulationEnded(ManipulationEventData data)
    {
        moving = false;
        return data;
    }

    //When user grabs marker, freeze cueball position and change hinge anchor
    ManipulationEventData cueMarkerManipulationStarted(ManipulationEventData data)
    {
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        cueMarker.GetComponent<HingeJoint>().anchor = cueMarker.transform.position + new Vector3(0.0f, 8.675f, 0.0f);
        return data;
    }

    //Once marker is let go, allow for ball to be moved gaain
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
        }
        this.cueMarker.SetActive(true);
        this.isCueBall = true;
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
