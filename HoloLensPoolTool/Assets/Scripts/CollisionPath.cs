using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;


public class CollisionPath : MonoBehaviour
{

    public GameObject ball;
    public int bounces;


    // Start is called before the first frame update
    void Start()
    {
        InitialiseLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.GetComponent<LineRenderer>().enabled == true) BallCollisionPath();
    }

    //Creates and adds a LineRenderer object to the connected GameObject
    void InitialiseLineRenderer()
    {
        LineRenderer rend = gameObject.AddComponent<LineRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        //Can scale by tables x scale, as only allowed proportional scale, so all axis scale by same amount
        float tableScale = GameObject.Find("Pool_Table").gameObject.transform.Find("Table").transform.localScale.x;
        rend.widthMultiplier = 0.75f * tableScale;
        rend.alignment = LineAlignment.TransformZ;
    }

    //Caluclates the points of collision
    void BallCollisionPath()
    {
        //Disable BallCollider so when path is drawn it doesnt detect collisions with itself
        ball.GetComponent<SphereCollider>().enabled = false;

        RaycastHit[] hits = new RaycastHit[bounces + 1];
        Vector3 startPos = ball.transform.position;


        Vector3 currentPos = startPos;
        Vector3 direction = this.transform.up;
        int hitsMade = 0;

        for(int i = 0; i < bounces + 1; i++)
        {
            if(Physics.Raycast(currentPos, direction, out hits[i]))
            {
                hitsMade++;
                //If it hits ball, calculate post collision path differently
                if(hits[i].collider.tag == "Ball")
                {
                    // Calculate ball to ball collision trajectory for both cue ball and targte ball (both for 1 bounce post collision)
                    break;
                }
                //if it hits the hit indicator collider, ignore it and continue on
                if(hits[i].collider.tag == "Cue_Hit_Marker")
                {
                    Vector3 markerPoint = hits[i].point;
                    Physics.Raycast(markerPoint, direction, out hits[i]);
                }
                currentPos = hits[i].point;
                direction = direction - 2 * (Vector3.Dot(direction, hits[i].normal)) * hits[i].normal;
            }
        }

        //Re-enable Ball's Collider so it can still be moved
        ball.GetComponent<SphereCollider>().enabled = true;

        DrawCollisionPath(startPos, hits, hitsMade);
    }

    //Draws the collision path from the calculated points
    void DrawCollisionPath(Vector3 startPos, RaycastHit[] hits, int hitsMade)
    {
        //Avoids floating line error
        if (hitsMade == 0) return;
        //Get renderer only once for more efficient computing
        LineRenderer rend = GetComponent<LineRenderer>();

        //Account for ball size so path drawn onto table surface
        float ballHeightOffset = ball.GetComponent<SphereCollider>().radius * ball.transform.parent.localScale.x;

        rend.positionCount = hitsMade + 1;
        Vector3[] positions = new Vector3[rend.positionCount];
        positions[0] = startPos - new Vector3(0.0f, ballHeightOffset, 0.0f);
        for (int i = 0; i < hitsMade; i++)
        {
            positions[i + 1] = hits[i].point - new Vector3(0.0f, ballHeightOffset, 0.0f);
        }

        rend.SetPositions(positions);
    }

    void ballToBallCollision()
    {

    }
}
