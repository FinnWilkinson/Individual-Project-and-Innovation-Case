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
        //if(this.GetComponent<LineRenderer>().enabled == true) BallCollisionPath();
        if(this.GetComponent<LineRenderer>().enabled == true) BallCollisionPathEstimation();
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

        //Set layerMask so that raycast doesnt interact or detect unwanted physics layers
        //Unwanted  = Layer 8 : ball_marker
        int layerMask = unchecked((int)0xFFFFFFFF - (1<< 8));

        RaycastHit[] hits = new RaycastHit[bounces + 1];
        Vector3 startPos = ball.transform.position;


        Vector3 currentPos = startPos;
        Vector3 direction = this.transform.up;
        int hitsMade = 0;

        for(int i = 0; i < bounces + 1; i++)
        {
            if(Physics.Raycast(currentPos, direction, out hits[i], Mathf.Infinity, layerMask))
            {
                hitsMade++;
                //If it hits ball, calculate post collision path differently
                if (hits[i].collider.tag == "Ball")
                {
                    // Calculate ball to ball collision trajectory for both cue ball and targte ball (both for 1 bounce post collision)
                    break;
                }
                currentPos = hits[i].point;
                direction = direction - 2 * (Vector3.Dot(direction, hits[i].normal)) * hits[i].normal;
            }
        }

        //Re-enable Ball's Collider so it can still be moved
        ball.GetComponent<SphereCollider>().enabled = true;

        Vector3[] hitPoints = new Vector3[hitsMade];
        for(int i = 0; i < hitsMade; i++)
        {
            hitPoints[i] = hits[i].point;
        }

        DrawCollisionPath(startPos, hitPoints, hitsMade);
    }

    //Caluclates the points of collision on cue balls path
    void BallCollisionPathEstimation()
    {
        //Disable BallCollider so when path is drawn it doesnt detect collisions with itself
        ball.GetComponent<SphereCollider>().enabled = false;

        //get global radius of ball for later use
        float radius = ball.transform.parent.transform.localScale.x * ball.GetComponent<SphereCollider>().radius;

        //Set layerMask so that raycast doesnt interact or detect unwanted physics layers
        //Unwanted  = Layer 8 : ball_marker, Layer 11 : Cue_Ball
        int layerMask = unchecked((int)0xFFFFFFFF - (1 << 8));
        layerMask -= (1 << 11);

        //initialise start variables
        Vector3 startPos = ball.transform.position;
        //RaycastHit[] hits = new RaycastHit[bounces + 1];

        //initialise loop variables
        int hitsMade = 0;
        Vector3 direction = this.transform.up;
        Vector3 dirNormal = Vector3.Cross(direction, Vector3.up); //Get normal to ray in y axis (imagine plane edge made through direction, normal to that plane)
        Vector3[] currentPos = {startPos, startPos - (dirNormal * radius), startPos + (dirNormal * radius)};  // (0 = middle, 1 = left side of ball, 2 = right side of ball)
        
        RaycastHit[] initialHits = new RaycastHit[3];
        Vector3[] hitPoints = new Vector3[bounces + 2];

        // Loop through to find all collisions
        for (int j = 0; j < bounces + 1; j++)
        {
            //find intial collisions
            if (Physics.Raycast(currentPos[0], direction, out initialHits[0], Mathf.Infinity, layerMask) &&
                Physics.Raycast(currentPos[1], direction, out initialHits[1], Mathf.Infinity, layerMask) &&
                Physics.Raycast(currentPos[2], direction, out initialHits[2], Mathf.Infinity, layerMask))
            {
                // if all tags same, either hits cushion or ball full on, 
                if (initialHits[0].collider.tag == initialHits[1].collider.tag && initialHits[0].collider.tag == initialHits[2].collider.tag)
                {
                    // if cushion, update positions, store hit position and move on to next bounce
                    if (initialHits[0].collider.tag == "Cushion")
                    {
                        //get new direction vector
                        direction = direction - 2 * (Vector3.Dot(direction, initialHits[0].normal)) * initialHits[0].normal;
                        dirNormal = Vector3.Cross(direction, Vector3.up);
                        //store hit point to draw path
                        hitPoints[hitsMade] = initialHits[0].point;
                        hitsMade++;
                        //update current firing positions
                        currentPos[0] = initialHits[0].point;
                        currentPos[1] = currentPos[0] - (dirNormal * radius);
                        currentPos[2] = currentPos[0] + (dirNormal * radius);
                    }
                    //if ball, find 1 next collision after ball and add this point, then break (as will hit ball full face so straight path post hit)
                    else if (initialHits[0].collider.tag == "Ball")
                    {
                        //store hit point to draw path
                        hitPoints[hitsMade] = initialHits[0].point;
                        hitsMade++;
                        //update current firing positions
                        currentPos[0] = initialHits[0].point;
                        currentPos[1] = currentPos[0] - (dirNormal * radius);
                        currentPos[2] = currentPos[0] + (dirNormal * radius);
                        if (Physics.Raycast(currentPos[0], direction, out initialHits[0], Mathf.Infinity, layerMask))
                        {
                            //store hit point to draw path
                            hitPoints[hitsMade] = initialHits[0].point;
                            hitsMade++;
                            break;
                        }
                    }
                    //else, something is wrong, dont draw at all
                    else return;
                }
                //if tags not all same, then a ball is hit at an angle
                else
                {
                    //if none hit ball, error so dont draw
                    if (initialHits[0].collider.tag != "Ball" && initialHits[1].collider.tag != "Ball" && initialHits[2].collider.tag != "Ball") return;

                    //find collision point
                    //work out this balls trajectory after (0-1 bounces)
                    //work out target ball trajectory after (0-1 bounces)
                    //break
                }
            }
        }
        
        //Re-enable Ball's Collider so it can still be moved
        ball.GetComponent<SphereCollider>().enabled = true;

        DrawCollisionPath(startPos, hitPoints, hitsMade);
    }

    //Draws the collision path from the calculated points
    void DrawCollisionPath(Vector3 startPos, Vector3[] hitPoints, int hitsMade)
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
            positions[i + 1] = hitPoints[i] - new Vector3(0.0f, ballHeightOffset, 0.0f);
        }

        rend.SetPositions(positions);
    }
}
