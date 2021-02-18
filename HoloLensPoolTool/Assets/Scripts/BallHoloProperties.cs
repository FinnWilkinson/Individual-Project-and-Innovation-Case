using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class BallHoloProperties : MonoBehaviour
{
    private bool boundsCreated;
    private float ballHeight;

    // Start is called before the first frame update
    void Start()
    {
        boundsCreated = false;
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
            //Get ball height so it can be fixed whilst user moves ball into position on table
            ballHeight = this.transform.position.y;
            boundsCreated = true;
        }        
    }

    //Sets bounds on positions ball can be - i.e. keeps ball on table surface bounds
    void PositionBounds()
    {
        this.transform.position = new Vector3(this.transform.position.x, ballHeight, this.transform.position.z);
        this.transform.rotation = new Quaternion(0,0,0,0);

    }
}
