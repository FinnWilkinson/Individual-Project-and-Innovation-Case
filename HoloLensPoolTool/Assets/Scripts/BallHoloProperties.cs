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
        //rigidbody temporarily used to get ball to be placed exactly onto table surface.
        Destroy(this.GetComponent<Rigidbody>());
    }
}
