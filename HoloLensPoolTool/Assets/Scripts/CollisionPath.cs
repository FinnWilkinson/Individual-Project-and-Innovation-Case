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
    public GameObject ball;     // Ball object that marker is child of
    public int bounces;         // Number of bounces we want to calculate

    private float radius;       // Radius of the ball in global scale
    private int layerMask;      // Mask of physics layers we dont want raycast to detect

    // Constants needed for cushion rebound path calculations
    private const float initalVelocity = 1.4f;  // Assumed velocity before impact - based on medium powered shot
    private const float coefOfRestitution = 0.54f;   // Coefficient of restitution of the table's cushions

    // Start is called before the first frame update
    void Start()
    {
        // Get global radius of ball for later use
        radius = ball.transform.parent.transform.localScale.x * ball.GetComponent<SphereCollider>().radius;

        // Set layerMask so that raycast doesnt interact or detect unwanted physics layers
        // Unwanted  = Layer 8 : ball_marker, Layer 11 : Cue_Ball
        layerMask = unchecked((int)0xFFFFFFFF - (1 << 8)); layerMask -= (1 << 11);

        // Initialise the line renderer
        InitialiseLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        // Draw ball collision path if connected ball is the cue ball
        if(this.GetComponent<LineRenderer>().enabled == true) BallCollisionPathEstimation();
    }

    // Creates and adds a LineRenderer object to the connected GameObject
    void InitialiseLineRenderer()
    {
        LineRenderer rend = gameObject.AddComponent<LineRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        // Can scale by tables x scale, as only allowed proportional scale, so all axis scale by same amount
        float tableScale = GameObject.Find("Pool_Table").gameObject.transform.Find("Table").transform.localScale.x;
        rend.widthMultiplier = 0.75f * tableScale;
        rend.alignment = LineAlignment.TransformZ;
    }

    // Caluclates the points of collision on cue balls path, as well as any ball it hits
    void BallCollisionPathEstimation()
    {
        // Initialise start position
        Vector3 startPos = ball.transform.position;

        // Initialise loop variables
        Vector3 direction = this.transform.up;      // Direction ball will be hit
        Vector3 dirNormal = Vector3.Cross(direction, Vector3.up); // Get normal to ray in y axis (imagine plane edge made through direction, normal to that plane)
        Vector3[] currentPos = {startPos, startPos - (dirNormal * radius), startPos + (dirNormal * radius)};  // (0 = middle, 1 = left side of ball, 2 = right side of ball)
        RaycastHit[] initialHits = new RaycastHit[3];   // Stores collision data for each of the three rays fired
        Vector3[] hitPoints = new Vector3[bounces + 2];     // Stores valid path points we want to draw between 
        int hitsMade = 0;       // Tracks number of hit points stored

        // Variables used to help control which object ball should have line renderer enables (which ball is being hit)
        GameObject lastHitBall = this.ball;
        bool anytimeBallHit = false;

        // Loop to find all collisions in n-bounces
        for (int j = 0; j < bounces + 1; j++)
        {
            // Find intial collisions from centre of cue ball. If no collision detected then dont continue
            if (Physics.Raycast(currentPos[0], direction, out initialHits[0], Mathf.Infinity, layerMask))
            {
                // Variables to keep track of which raycasts have successfully hit something, and if they have hit balls
                bool ballHitL = false, ballHitR = false, ballHitC = false;
                bool leftSideHit = false, rightSideHit = false;

                // Fire raycasts either side of cue ball to see if we will collide with another ball not down centre line
                if (Physics.Raycast(currentPos[1], direction, out initialHits[1], Mathf.Infinity, layerMask))
                {
                    leftSideHit = true;
                }
                if (Physics.Raycast(currentPos[2], direction, out initialHits[2], Mathf.Infinity, layerMask))
                {
                    rightSideHit = true;
                }

                // Check which rays hit balls
                if (initialHits[0].collider.tag == "Ball") ballHitC = true;
                if (leftSideHit == true && initialHits[1].collider.tag == "Ball") ballHitL = true;
                if (rightSideHit == true && initialHits[2].collider.tag == "Ball") ballHitR = true;

                // Store the previous point on path that cue ball would be before this collision
                Vector3 prevPoint = ball.transform.position;
                if (hitsMade > 0) prevPoint = hitPoints[hitsMade - 1];

                // If no ball is hit from any raycast, we only hit cushion; go from centre hit point
                if (!ballHitL && !ballHitR && !ballHitC)
                {
                    // If a pocket was hit on cue ball's path, dont figure out any more collisions
                    if (initialHits[0].collider.tag == "Pocket")
                    {
                        // Store hit point
                        hitPoints[hitsMade] = initialHits[0].point;
                        hitsMade++;
                        break;
                    }

                    // Get normal to surface we hit (need to invert as points outwards)
                    Vector3 surfaceNormal = -initialHits[0].normal;
                    // Get the angle between the cushion normal and the inbound cue ball path
                    float normalDirAngle = Vector3.SignedAngle(direction, surfaceNormal, Vector3.up);
                    // Calculate the angle between inbound cue ball path and cushion
                    float alpha = (90 - Mathf.Abs(normalDirAngle)) * (Mathf.PI / 180);

                    // Calculate velocity in direction parallel to cushion immediatly after collision
                    float postColVelocityTangent = initalVelocity * Mathf.Cos(alpha);
                    //Calculate velocity in direction parallel to cushion normal immediatly after collision
                    float postColVelocityNormal = coefOfRestitution * (initalVelocity * Mathf.Sin(alpha));
                    // Calculate the angle between cushion and outbound direction of cue ball after the collision
                    float beta = Mathf.Abs(Mathf.Atan(postColVelocityNormal / postColVelocityTangent));

                    // Find reverse of inbound direction vector
                    Vector3 reverseVector = new Vector3(prevPoint.x - initialHits[0].point.x, 0.0f, prevPoint.z - initialHits[0].point.z);

                    // Calculate Rotation
                    float newX, newZ;
                    float rotationAngle = -(Mathf.PI - alpha - beta);
                    if (normalDirAngle < 0) rotationAngle = -rotationAngle;
                    newX = (reverseVector.x) * Mathf.Cos(rotationAngle) - (reverseVector.z) * Mathf.Sin(rotationAngle);
                    newZ = (reverseVector.x) * Mathf.Sin(rotationAngle) + (reverseVector.z) * Mathf.Cos(rotationAngle);

                    // Update Direction Vector and its normal
                    direction = new Vector3(newX, direction.y, newZ);   // y stays the same as balls all at the exact same height
                    dirNormal = Vector3.Cross(direction, Vector3.up);

                    // Store hit point
                    hitPoints[hitsMade] = initialHits[0].point;
                    hitsMade++;
                    // Update raycast firing positions
                    currentPos[0] = initialHits[0].point;
                    currentPos[1] = initialHits[0].point - (dirNormal * radius);
                    currentPos[2] = initialHits[0].point + (dirNormal * radius);
                    // Continue round loop to calculate next collision/bounce
                }
                // Else, we hit a ball so calculate path appropriatly
                else
                {
                    // For all calculations, can emmit y as this will be the same for every ball (all same size and on a level plane)
                    anytimeBallHit = true;

                    // Store list of all balls that were hit
                    List<GameObject> ballsHit = new List<GameObject>();
                    if (ballHitC) ballsHit.Add(initialHits[0].collider.gameObject);
                    if (ballHitL) ballsHit.Add(initialHits[1].collider.gameObject);
                    if (ballHitR) ballsHit.Add(initialHits[2].collider.gameObject);

                    // Find which ball that we hit is closest to the cue ball
                    GameObject closestBall = ballsHit[0];
                    if (ballsHit.Count > 1)
                    {
                        for (int i = 1; i < ballsHit.Count; i++)
                        {
                            float closestDist = Mathf.Sqrt(Mathf.Pow(closestBall.transform.position.x - prevPoint.x, 2) + Mathf.Pow(closestBall.transform.position.z - prevPoint.z, 2));
                            float checkDist = Mathf.Sqrt(Mathf.Pow(ballsHit[i].transform.position.x - prevPoint.x, 2) + Mathf.Pow(ballsHit[i].transform.position.z - prevPoint.z, 2));
                            if (checkDist < closestDist) closestBall = ballsHit[i];
                        }
                    }

                    // Update which ball should have line renderer enabled
                    if(lastHitBall != closestBall)
                    {
                        foreach (GameObject x in GameObject.FindGameObjectsWithTag("Ball"))
                        {
                            x.GetComponent<BallProperties>().disbaleLineRenderer();
                        }
                        lastHitBall = closestBall;
                        lastHitBall.GetComponent<BallProperties>().enableLineRenderer();
                    }

                    // Initialise centre points of cue ball's current position, the ball we have hit, and where cue ball will be when collision occurs
                    Vector3 cueBallCentre = prevPoint;
                    Vector3 hitBallCentre = closestBall.transform.position;
                    Vector3 cueBallCollCentre = new Vector3();

                    // If z coordinates the approx. same, then plug into circle equation immediatly to get x positions of cue ball centre point on collision, as line is horizontal
                    if (Mathf.Abs(cueBallCentre.z - initialHits[0].point.z) <= 0.01)
                    {
                        // (x-a)^2 + (z-b)^2 = (2*radius)^2         where a and b are x and z coords repectivley of hitBallCentre
                        // simplify and find x
                        float[] potentialXcoords = new float[2];
                        float q = Mathf.Pow(hitBallCentre.x, 2) + Mathf.Pow(cueBallCentre.z, 2) - (2 * hitBallCentre.z * cueBallCentre.z) + Mathf.Pow(hitBallCentre.z, 2) - Mathf.Pow(2 * radius, 2);
                        potentialXcoords[0] = -(((-2 * hitBallCentre.x) + Mathf.Sqrt(Mathf.Pow(-2 * hitBallCentre.x, 2) - (4 * q))) / (2));    //quadratic formula with +
                        potentialXcoords[1] = -(((-2 * hitBallCentre.x) - Mathf.Sqrt(Mathf.Pow(-2 * hitBallCentre.x, 2) - (4 * q))) / (2));    //quadratic formula with -

                        // Find which of the 2 possible points is closest to cue ball, and will therefore be the correct coordinates
                        if (Mathf.Sqrt(Mathf.Pow(potentialXcoords[0] - cueBallCentre.x, 2) + Mathf.Pow(cueBallCentre.z - cueBallCentre.z, 2)) <
                            Mathf.Sqrt(Mathf.Pow(potentialXcoords[1] - cueBallCentre.x, 2) + Mathf.Pow(cueBallCentre.z - cueBallCentre.z, 2)))
                            cueBallCollCentre = new Vector3(potentialXcoords[0], cueBallCentre.y, cueBallCentre.z);
                        else cueBallCollCentre = new Vector3(potentialXcoords[1], cueBallCentre.y, cueBallCentre.z);
                    }
                    // If x coordinates the approx. same, then plug into circle equation immediatly to get z positions of cue ball centre point on collision, as line is verticle (undefined gradient)
                    else if (Mathf.Abs(cueBallCentre.x - initialHits[0].point.x) <= 0.01) 
                    {
                        // (x-a)^2 + (z-b)^2 = (2*radius)^2         where a and b are x and z coords repectivley of hitBallCentre
                        // simplify and find z
                        float[] potentialZcoords = new float[2];
                        float q = Mathf.Pow(cueBallCentre.x, 2) - (2 * hitBallCentre.x * cueBallCentre.x) + Mathf.Pow(hitBallCentre.x, 2) + Mathf.Pow(hitBallCentre.z, 2) - Mathf.Pow(2 * radius, 2);
                        potentialZcoords[0] = -(((-2 * hitBallCentre.z) + Mathf.Sqrt(Mathf.Pow(-2 * hitBallCentre.z, 2) - (4 * q))) / (2));    //quadratic formula with +
                        potentialZcoords[1] = -(((-2 * hitBallCentre.z) - Mathf.Sqrt(Mathf.Pow(-2 * hitBallCentre.z, 2) - (4 * q))) / (2));    //quadratic formula with -

                        // Find which of the 2 possible points is closest to cue ball, and will therefore be the correct coordinates
                        if (Mathf.Sqrt(Mathf.Pow(cueBallCentre.x - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[0] - cueBallCentre.z, 2)) <
                            Mathf.Sqrt(Mathf.Pow(cueBallCentre.x - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[1] - cueBallCentre.z, 2)))
                            cueBallCollCentre = new Vector3(cueBallCentre.x, cueBallCentre.y, potentialZcoords[0]);
                        else cueBallCollCentre = new Vector3(cueBallCentre.x, cueBallCentre.y, potentialZcoords[1]);
                    }
                    // Else, calculate z=mx+c line for centre raycast and ball we hit's circle equation (of double radius) to calculate the intersection points and hence 
                    // cue ball's centre point at time of collision
                    else
                    {
                        // Get line equation of centre ray
                        // z = mx+c
                        float gradient = (cueBallCentre.z - initialHits[0].point.z) / (cueBallCentre.x - initialHits[0].point.x);
                        float intercept = cueBallCentre.z - gradient * cueBallCentre.x;

                        // Plug z=mx+c into circle equation and simplify to get x intercept coordinates (exactly 2)
                        // Circle equation => (x-a)^2 + (z-b)^2 = radius^2          where a=x and b=z coordinates of hitBallCentre
                        // We want to find centre of cue ball on contact, so radius of circle equation is 2*ball_radius
                        // Maths below is simplified version of (x-a)^2 + (mx+c-b)^2 = (2*radius)^2  =>  tx^2 - ux - v = 0, with t, u, v representing summations of constants
                        float[] potentialXcoords = new float[2], potentialZcoords = new float[2];
                        float t = 1 + (gradient * gradient);
                        float u = 2 * (hitBallCentre.x - (gradient * (intercept - hitBallCentre.z)));
                        float v = (4 * (radius * radius)) - Mathf.Pow(hitBallCentre.x, 2) - Mathf.Pow(intercept - hitBallCentre.z, 2);
                        // Use quadratic formula to get both possible x solutions
                        potentialXcoords[0] = -(((-u) + Mathf.Sqrt((u * u) - (4 * t * (-v)))) / (2 * t));       // Quadratic formula with +
                        potentialXcoords[1] = -(((-u) - Mathf.Sqrt((u * u) - (4 * t * (-v)))) / (2 * t));       // Quadratic formula with -
                        // Plug both x coordinates we found into z=mx+c to get both possible z coordinates
                        potentialZcoords[0] = gradient * potentialXcoords[0] + intercept;
                        potentialZcoords[1] = gradient * potentialXcoords[1] + intercept;

                        // Find which of the 2 possible points is closest to cue ball, and will therefore be the correct coordinates
                        if (Mathf.Sqrt(Mathf.Pow(potentialXcoords[0] - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[0] - cueBallCentre.z, 2)) <
                            Mathf.Sqrt(Mathf.Pow(potentialXcoords[1] - cueBallCentre.x, 2) + Mathf.Pow(potentialZcoords[1] - cueBallCentre.z, 2)))
                            cueBallCollCentre = new Vector3(potentialXcoords[0], cueBallCentre.y, potentialZcoords[0]);
                        else cueBallCollCentre = new Vector3(potentialXcoords[1], cueBallCentre.y, potentialZcoords[1]);
                    }

                    // Add centre point of where cue ball will be at collision to hitPoints so is drawn onto path
                    hitPoints[hitsMade] = cueBallCollCentre;
                    hitsMade++;

                    // Calculate direction of hit balls trajectory post collision (vector going from centre of cue ball position at collision through centre of ball we hit)
                    Vector3 hitBallDirection = hitBallCentre - cueBallCollCentre;
                    // Invokle object ball's line drawing function to draw correct post collision path (0 bounces, 1 collision)
                    closestBall.GetComponent<BallProperties>().DrawCollisionPath(hitBallDirection);

                    // if angle between cue ball direction pre collision and hitBallDirection < 10 degrees, then dont draw post collision cue ball line as will continue straight
                    float angleBetween = Vector3.Angle(direction, hitBallDirection);
                    if (angleBetween < 10) break;

                    // Rotate hit ball's post collision vector 90 degrees to get cue ball's post collision direction vector
                    float newX, newZ;
                    // If right side of cue ball hits, rotate anti-clockwise
                    if (ballHitR && closestBall == initialHits[2].collider.gameObject)
                    {
                        newX = hitBallDirection.x * Mathf.Cos(-Mathf.PI / 2) - hitBallDirection.z * Mathf.Sin(-Mathf.PI / 2);
                        newZ = hitBallDirection.x * Mathf.Sin(-Mathf.PI / 2) + hitBallDirection.z * Mathf.Cos(-Mathf.PI / 2);
                    }
                    // If left side of cue ball hits (or centre hit), rotate clockwise
                    else
                    {
                        newX = hitBallDirection.x * Mathf.Cos(Mathf.PI / 2) - hitBallDirection.z * Mathf.Sin(Mathf.PI / 2);
                        newZ = hitBallDirection.x * Mathf.Sin(Mathf.PI / 2) + hitBallDirection.z * Mathf.Cos(Mathf.PI / 2);
                    }

                    // Update cue ball's direction vector and firing position
                    direction = new Vector3(newX, direction.y, newZ);
                    dirNormal = Vector3.Cross(direction, Vector3.up);
                    currentPos[0] = cueBallCollCentre;

                    // Calculate cue balls path after collision (0 bounce, 1 more collision)
                    RaycastHit hit;
                    if (Physics.Raycast(currentPos[0], direction, out hit, Mathf.Infinity, layerMask))
                    {
                        hitPoints[hitsMade] = hit.point;
                        hitsMade++;
                    }
                    else break;     // If raycast fails, do nothing

                    break;      // Stop loop as all collision points have been calculated
                }
            }
        }

        // If no balls are hit at any point, ensure all object ball line renderers are off
        // Needed for when previously have hit an object ball, then rotate path to cushion only collisions
        if (!anytimeBallHit)
        {
            foreach (GameObject x in GameObject.FindGameObjectsWithTag("Ball"))
            {
                x.GetComponent<BallProperties>().disbaleLineRenderer();
            }
        }

        // Draw path
        DrawCollisionPath(startPos, hitPoints, hitsMade);
    }

    // Draws the collision path from the calculated points
    void DrawCollisionPath(Vector3 startPos, Vector3[] hitPoints, int hitsMade)
    {
        // Avoids floating line error
        if (hitsMade == 0) return;
        // Get renderer only once for more efficient computing
        LineRenderer rend = GetComponent<LineRenderer>();

        // Account for ball size so path drawn onto table surface instead of in mid air
        float ballHeightOffset = ball.GetComponent<SphereCollider>().radius * ball.transform.parent.localScale.x;

        // Initialise Line renderer positions
        rend.positionCount = hitsMade + 1;
        // Vector to store all positions we want to draw between
        Vector3[] positions = new Vector3[rend.positionCount];

        // Add cue ball start position to position vector, reducing hight so drawn on top of table surface
        positions[0] = startPos - new Vector3(0.0f, ballHeightOffset, 0.0f);
        for (int i = 0; i < hitsMade; i++)
        {
            // Add each of the calculated positions to vector, reducing all their heights so line is drawn on top of table surface
            positions[i + 1] = hitPoints[i] - new Vector3(0.0f, ballHeightOffset, 0.0f);
        }

        // Set line positions so that it is actually drawn in scene
        rend.SetPositions(positions);
    }
}
