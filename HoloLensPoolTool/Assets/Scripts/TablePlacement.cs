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


        saveBtn.GetComponent<Interactable>().OnClick.AddListener(() => SavePos());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SavePos()
    {
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
            //if(child.parent.name == table.name && child.name != "rigRoot") child.GetComponent<MeshRenderer>().enabled = false;
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
