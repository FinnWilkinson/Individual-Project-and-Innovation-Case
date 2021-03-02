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
        BallCollisionPath();
    }

    //Creates and adds a LineRenderer object to the connected GameObject
    void InitialiseLineRenderer()
    {
        LineRenderer rend = gameObject.AddComponent<LineRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        //Can scale by tables x scale, as only allowed proportional scale, so all axis scale by same amount
        rend.widthMultiplier = 0.01f * GameObject.Find("Pool_Table").gameObject.transform.Find("Table").transform.localScale.x;
    }

    //Caluclates the points of collision
    void BallCollisionPath()
    {
        RaycastHit[] hits = new RaycastHit[bounces + 1];

        //for()


        Physics.Raycast(ball.transform.position, this.transform.up, out hits[0]);
        DrawCollisionPath(hits);
    }

    //Draws the collision path from the calculated points
    void DrawCollisionPath(RaycastHit[] hits)
    {

        LineRenderer rend = GetComponent<LineRenderer>();

        Color lineColour = Color.white;
        float lengthOfLine1 = Mathf.Sqrt(Mathf.Pow(ball.transform.position.x - hits[0].point.x, 2)
                                                                    +
                                         Mathf.Pow(ball.transform.position.z - hits[0].point.z, 2));

        Vector3[] positions = {ball.transform.position, hits[0].point};

        rend.SetPositions(positions);
    }
}
