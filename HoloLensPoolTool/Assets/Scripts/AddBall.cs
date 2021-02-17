using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class AddBall : MonoBehaviour
{
    public GameObject table_surface;
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
        Vector3 ball_Pos = table_surface.transform.position + new Vector3(0, 0.4f, 0);
        Instantiate(ball_Prefab, ball_Pos, Quaternion.identity);
    }
}
