using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHoloProperties : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        //this.GetComponent<Rigidbody>().useGravity = false;
        Destroy(this.GetComponent<Rigidbody>());
    }
}
