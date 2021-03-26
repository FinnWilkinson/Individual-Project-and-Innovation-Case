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
        int layerMask = unchecked((int)0xFFFFFFFF - (1 << 8)); layerMask -= (1 << 11);

        //initialise start variables
        Vector3 startPos = ball.transform.position;

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
            if (Physics.Raycast(currentPos[0], direction, out initialHits[0], Mathf.Infinity, layerMask))
            {
                bool ballHitL = false, ballHitR = false, ballHitC = false;
                bool leftSideHit = false, rightSideHit = false;

                //Fire rays either side of ball to see if we will collide with another ball not down centre line
                if (Physics.Raycast(currentPos[1], direction, out initialHits[1], Mathf.Infinity, layerMask))
                {
                    leftSideHit = true;
                }
                if (Physics.Raycast(currentPos[2], direction, out initialHits[2], Mathf.Infinity, layerMask))
                {
                    rightSideHit = true;
                }

                //check which rays hit balls
                if (initialHits[0].collider.tag == "Ball")
                {
                    ballHitC = true;
                    Debug.Log("centre hit");
                }
                if (leftSideHit == true)
                {
                    if (initialHits[1].collider.tag == "Ball")
                    {
                        ballHitL = true;
                        Debug.Log("left hit");
                    }

                }
                if (rightSideHit == true)
                {
                    if (initialHits[2].collider.tag == "Ball")
                    {
                        ballHitR = true;
                        Debug.Log("right hit");
                    }
                }

                //If no ball hit, go from centre hit point as hit cushion
                if (!ballHitL && !ballHitR && !ballHitC)
                {
                    //get new direction vector
                    direction = direction - 2 * (Vector3.Dot(direction, initialHits[0].normal)) * initialHits[0].normal;
                    dirNormal = Vector3.Cross(direction, Vector3.up);
                    //store hit point to draw path
                    hitPoints[hitsMade] = initialHits[0].point;
                    hitsMade++;
                    //update current firing positions
                    currentPos[0] = initialHits[0].point;
                    currentPos[1] = initialHits[0].point - (dirNormal * radius);
                    currentPos[2] = initialHits[0].point + (dirNormal * radius);
                }
                //else do ball collision stuff
                else
                {
                    // For all calculations, can emmit y as this will be the same for every ball (all same size on a flat plane)
                    // Find closest ball which was hit
                    List<GameObject> ballsHit = new List<GameObject>();
                    GameObject closestBall;
                    if (ballHitC) ballsHit.Add(initialHits[0].collider.gameObject);
                    if (ballHitL) ballsHit.Add(initialHits[1].collider.gameObject);
                    if (ballHitR) ballsHit.Add(initialHits[2].collider.gameObject);

                    Vector3 prevPoint = ball.transform.position;
                    if (hitsMade > 0) prevPoint = hitPoints[hitsMade - 1];

                    closestBall = ballsHit[0];
                    if (ballsHit.Count > 1)
                    {
                        for (int i = 1; i < ballsHit.Count; i++)
                        {
                            float closestDist = Mathf.Sqrt(Mathf.Pow(closestBall.transform.position.x - prevPoint.x, 2) + Mathf.Pow(closestBall.transform.position.z - prevPoint.z, 2));
                            float checkDist = Mathf.Sqrt(Mathf.Pow(ballsHit[i].transform.position.x - prevPoint.x, 2) + Mathf.Pow(ballsHit[i].transform.position.z - prevPoint.z, 2));
                            if (checkDist < closestDist) closestBall = ballsHit[i];
                        }
                    }

                    // Find centre point of where cue ball will be at collision
                    Vector3 cueBallCentre = prevPoint;
                    Vector3 hitBallCentre = closestBall.transform.position;
                    Vector3 cueBallCollCentre = new Vector3();

                    //if either z or x coordinates the approx. same, then plug into circle equation immediatly to get cue ball centre point on collision
                    if (Mathf.Abs(cueBallCentre.z - initialHits[0].point.z) <= 0.01)
                    {
                        break;
                    }
                    else if (Mathf.Abs(cueBallCentre.x - initialHits[0].point.x) <= 0.01) 
                    {
                        break;
                    }
                    //else, calculate as normal
                    else
                    {
                        // Get line equation of centre ray
                        float gradient = (cueBallCentre.z - initialHits[0].point.z) / (cueBallCentre.x - initialHits[0].point.x);
                        float intercept = cueBallCentre.z - gradient * cueBallCentre.x;

                        //Plug into simplified circle equation to get x intercept coords
                        float[] potentialXcoords = new float[2], potentialZcoords = new float[2];
                        float t = 1 + (gradient * gradient);
                        float u = 2 * (hitBallCentre.x - (gradient * (intercept - hitBallCentre.z)));
                        float v = (4 * (radius * radius)) - Mathf.Pow(hitBallCentre.x, 2) - Mathf.Pow(intercept - hitBallCentre.z, 2);
                        potentialXcoords[0] = -(((-u) + Mathf.Sqrt((u * u) - (4 * t * (-v)))) / (2 * t));
                        potentialXcoords[1] = -(((-u) - Mathf.Sqrt((u * u) - (4 * t * (-v)))) / (2 * t));
                        potentialZcoords[0] = gradient * potentialXcoords[0] + intercept;
                        potentialZcoords[1] = gradient * potentialXcoords[1] + intercept;

                        if (Mathf.Sqrt(Mathf.Pow(potentialXcoords[0] - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[0] - cueBallCentre.z, 2)) <
                            Mathf.Sqrt(Mathf.Pow(potentialXcoords[1] - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[1] - cueBallCentre.z, 2)))
                            cueBallCollCentre = new Vector3(potentialXcoords[0], cueBallCentre.y, potentialZcoords[0]);
                        else cueBallCollCentre = new Vector3(potentialXcoords[1], cueBallCentre.y, potentialZcoords[1]);
                    }

                    //find point on object ball where collision occurs
                    //float ballZdiff = Mathf.Abs(cueBallCentre.z - hitBallCentre.z);
                    //float ballXdiff = Mathf.Abs(cueBallCentre.x - hitBallCentre.x);
                    //float angle = (Mathf.PI / 2) - Mathf.Asin(ballXdiff / (2 * radius));
                    //Vector3 collisionPoint = cueBallCentre + new Vector3(radius*Mathf.Sin(angle), 0.0f, radius*Mathf.Cos(angle));

                    //add centre point of where cue ball will be at collision to hitPoints
                    hitPoints[hitsMade] = cueBallCollCentre;
                    hitsMade++;

                    //calculate balls path afterwards (0 bounce)
                    //calculate cue balls path after collision (0 bounce)
                    break;
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
