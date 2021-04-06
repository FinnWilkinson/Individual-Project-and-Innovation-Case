/*Script used to save table position on button click and disable all scripts allowing for user to 
 * move, scale, rotate table. 
 * 
 * Table is a parent object holding all table pieces, allowing for cushion and table surface distinction.
 * 
 * Also hides save button after press (as one time use) and activates add ball button so user can put 
 * marker on balls.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

using UnityEngine.XR.WSA;

public class TablePlacement : MonoBehaviour, IMixedRealityInputHandler
{

    public GameObject saveBtn;
    public GameObject addBallBtn;
    public GameObject clearBallsBtn;
    public GameObject toggleBallUIBtn;
    public GameObject table;

    // Start is called before the first frame update
    void Start()
    {
        //ensure buttons are in correct starting state
        saveBtn.SetActive(true);
        addBallBtn.SetActive(false);
        clearBallsBtn.SetActive(false);
        toggleBallUIBtn.SetActive(false);

        //Set listeners to keep buttons with table when moved
        table.GetComponent<ManipulationHandler>().OnManipulationStarted.AddListener((input) => ManipStart(input));
        table.GetComponent<ManipulationHandler>().OnManipulationEnded.AddListener((data) => ManipEnd(data));

        //Set listeners to keep buttons with table when scaled
        table.GetComponent<BoundsControl>().RotateStarted.AddListener(() => RotateORScaleStart());
        table.GetComponent<BoundsControl>().RotateStopped.AddListener(() => RotateORScaleEnd());

        //Set listeners to keep buttons with table when rotated
        table.GetComponent<BoundsControl>().ScaleStarted.AddListener(() => RotateORScaleStart());
        table.GetComponent<BoundsControl>().ScaleStopped.AddListener(() => RotateORScaleEnd());

        //Set listeners for when save btn is pressed
        saveBtn.GetComponent<Interactable>().OnClick.AddListener(() => SavePos());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update Table UI transform parents when table moved so they stay relative to it
    ManipulationEventData ManipStart(ManipulationEventData data)
    {
        SetParentTable();
        return data;
    }

    // Revert Table UI transform parents after table moved so they stay relative to it
    ManipulationEventData ManipEnd(ManipulationEventData data)
    {
        SetParentThis();
        return data;
    }

    // Update Table UI transform parents when table rotated/scaled so they stay relative to it
    void RotateORScaleStart()
    {
        SetParentTable();
    }

    // Revert Table UI transform parents after table rotated/scaled so they stay relative to it
    void RotateORScaleEnd()
    {
        SetParentThis();
    }

    //Set table UI transform parent to table object
    void SetParentTable()
    {
        saveBtn.transform.parent = table.transform;
        addBallBtn.transform.parent = table.transform;
        clearBallsBtn.transform.parent = table.transform;
        toggleBallUIBtn.transform.parent = table.transform;
    }

    //Set table UI transform parent to prefab parent game object (this)
    void SetParentThis()
    {
        saveBtn.transform.parent = this.transform;
        addBallBtn.transform.parent = this.transform;
        clearBallsBtn.transform.parent = this.transform;
        toggleBallUIBtn.transform.parent = this.transform;
    }


    //Save table position, make correct UI available, disable un-needed table move scripts
    void SavePos()
    {
        // Add a world anchor to improve hologram stability within scene
        this.gameObject.AddComponent<WorldAnchor>();

        //de-activate scripts responsible for movement
        table.GetComponent<BoxCollider>().enabled = false;
        table.GetComponent<NearInteractionGrabbable>().enabled = false;
        table.GetComponent<ManipulationHandler>().enabled = false;
        table.GetComponent<ConstraintManager>().enabled = false;
        table.GetComponent<BoundsControl>().enabled = false;

        //dissable save btn, enable add ball btn & clear balls btn
        saveBtn.SetActive(false);
        addBallBtn.SetActive(true);
        clearBallsBtn.SetActive(true);
        toggleBallUIBtn.SetActive(true);

        //make table invisible
        foreach(Transform child in table.transform)
        {
            //if(child.parent.name == table.name && child.name != "rigRoot" && child.tag != "Pocket") child.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void IMixedRealityInputHandler.OnInputDown(InputEventData eventData)
    {
        foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
        {
            ballObj.GetComponent<BallPrefab>().SetSelected(false);
        }
    }

    void IMixedRealityInputHandler.OnInputUp(InputEventData eventData)
    {

    }
}
