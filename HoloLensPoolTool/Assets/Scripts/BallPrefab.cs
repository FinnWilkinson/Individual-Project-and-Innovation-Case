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

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        if (isCueBall)
        {
            cueMarker.transform.position = ball.transform.position + new Vector3(0.0f, 0.0f, -0.7f * this.transform.localScale.z);
        }
        //keep buttons with ball as it is moved.
        if (selected)
        {
            buttonsHolder.transform.position = ball.transform.position + new Vector3(0.0f, 2.0f * this.transform.localScale.y, 0.0f);
            buttonsHolder.transform.LookAt(mainCamera.transform);
        }
    }

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

    public void SetSelected(bool val)
    {
        if (allowUI)
        {
            selected = val;
            buttonsHolder.SetActive(val);
        }
    }

    void removeThisBall()
    {
        Destroy(this.gameObject);
    }
}
