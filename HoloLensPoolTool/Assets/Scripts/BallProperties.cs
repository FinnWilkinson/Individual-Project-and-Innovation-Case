using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class BallProperties : MonoBehaviour
{
    public GameObject lineRendObj;

    private bool boundsCreated;
    private float ballHeight;

    // Start is called before the first frame update
    void Start()
    {
        boundsCreated = false;
        InitialiseLineRenderer();
        lineRendObj.GetComponent<LineRenderer>().enabled = false;
        this.GetComponent<ObjectManipulator>().OnHoverEntered.AddListener((data) => HoverEntered(data));
    }

    // Update is called once per frame
    void Update()
    {
        if (boundsCreated)
        {
            PositionBounds();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody mine = this.GetComponent<Rigidbody>();
        if (mine.useGravity)
        {
            //rigidbody gravity temporarily used to get ball to be placed exactly onto table surface.
            mine.useGravity = false;
            mine.isKinematic = true;
            //Get ball height so it can be fixed whilst user moves ball into position on table
            ballHeight = this.transform.position.y;
            boundsCreated = true;
            this.transform.parent.transform.position = this.transform.position;
        }        
    }

    //Sets bounds on positions ball can be - i.e. keeps ball on table surface bounds
    void PositionBounds()
    {
        this.transform.position = new Vector3(this.transform.position.x, ballHeight, this.transform.position.z);
        this.transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    ManipulationEventData HoverEntered(ManipulationEventData data)
    {
        foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
        {
            ballObj.GetComponent<BallPrefab>().SetSelected(false);
        }
        this.transform.parent.gameObject.GetComponent<BallPrefab>().SetSelected(true);
        return data;
    }

    //sets up the LineRenderer object in the connected GameObject
    void InitialiseLineRenderer()
    {
        LineRenderer rend = lineRendObj.GetComponent<LineRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        //Can scale by tables x scale, as only allowed proportional scale, so all axis scale by same amount
        float tableScale = GameObject.Find("Pool_Table").gameObject.transform.Find("Table").transform.localScale.x;
        rend.widthMultiplier = 0.75f * tableScale;
        rend.alignment = LineAlignment.TransformZ;
        //rend.sharedMaterial.SetColor("_Color", Color.yellow);
    }

    //Draws the collision path from the given points
    public void DrawCollisionPath(Vector3 direction)
    {
        //Get renderer only once for more efficient computing
        LineRenderer rend = lineRendObj.GetComponent<LineRenderer>();
        rend.positionCount = 2;
        Vector3[] positions = new Vector3[2];

        //Account for ball size so path drawn onto table surface
        float ballHeightOffset = this.GetComponent<SphereCollider>().radius * this.transform.parent.localScale.x;

        //set start pos
        positions[0] = this.transform.position - new Vector3(0.0f, ballHeightOffset, 0.0f);

        //Set layerMask so that raycast doesnt interact or detect unwanted physics layers
        //Unwanted  = Layer 8 : ball_marker, Layer 11 : Cue_Ball
        int layerMask = unchecked((int)0xFFFFFFFF - (1 << 8)); layerMask -= (1 << 11);

        RaycastHit hit;

        //fire single raycast to get collision point
        if (Physics.Raycast(this.transform.position, direction, out hit, Mathf.Infinity, layerMask))
        {
            positions[1] = hit.point - new Vector3(0.0f, ballHeightOffset, 0.0f);
        }
        else return;

        rend.SetPositions(positions);
    }

}
