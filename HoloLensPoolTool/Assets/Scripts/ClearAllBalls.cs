using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class ClearAllBalls : MonoBehaviour
{
    public GameObject clearAllBtn;

    // Start is called before the first frame update
    void Start()
    {
        clearAllBtn.GetComponent<Interactable>().OnClick.AddListener(() => ClearAllCurrentBalls());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ClearAllCurrentBalls()
    {
        var listBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(GameObject ball in listBalls)
        {
            Destroy(ball);
        }
    }
}
