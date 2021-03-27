using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class AddBall : MonoBehaviour
{
    public GameObject table;
    public GameObject ball_Prefab;
    public GameObject addBtn;

    // Start is called before the first frame update
    void Start()
    {
        addBtn.GetComponent<Interactable>().OnClick.AddListener(() => addBall()) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void addBall()
    {
        //Position the Ball on or just above table surface.
        //in y coord, we add on (0.25f * scale_of_table) to ensure that the ball is always placed above the table surface no matter the rotation, scaling, or movement done by the user for alignment.
        //The ball_prefab is fitted with rigid body component to animate it falling onto the table surface. This is so the ball is exactly ontop of the table surface. After contact, the rigidbody is removed
        //So the ball no longer acts under gravity or any other forces.
        float scale = table.transform.localScale.y;
        Vector3 ball_Pos = table.transform.position + new Vector3(0, (0.25f * scale), 0); 
        Instantiate(ball_Prefab, ball_Pos, Quaternion.identity, this.transform);
    }
}
