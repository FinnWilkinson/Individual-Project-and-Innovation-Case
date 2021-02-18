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

public class TablePlacement : MonoBehaviour
{

    public GameObject saveBtn;
    public GameObject addBallBtn;
    // Start is called before the first frame update
    void Start()
    {
        saveBtn.GetComponent<Interactable>().OnClick.AddListener(() => SavePos());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SavePos()
    {
        //de-activate scripts responsible for movement
        this.GetComponent<BoxCollider>().enabled = false;
        this.GetComponent<NearInteractionGrabbable>().enabled = false;
        this.GetComponent<ManipulationHandler>().enabled = false;
        this.GetComponent<ConstraintManager>().enabled = false;
        this.GetComponent<BoundsControl>().enabled = false;

        //dissable save btn, enable add ball btn
        saveBtn.SetActive(false);
        addBallBtn.SetActive(true);

        //make table invisible
        foreach(Transform child in this.transform)
        {
            if(child.parent.name == this.name && child.name != "rigRoot") child.GetComponent<MeshRenderer>().enabled = false;
        }

    }
}
