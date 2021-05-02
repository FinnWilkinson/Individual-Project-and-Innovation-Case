using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class ToggleBallUI : MonoBehaviour
{
    public GameObject toggleBallUIBtn;

    // Start is called before the first frame update
    void Start()
    {
        toggleBallUIBtn.GetComponent<Interactable>().OnClick.AddListener(() => hideBallUI());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void hideBallUI()
    {
        if (toggleBallUIBtn.GetComponent<Interactable>().IsToggled)
        {
            //if toggled, Hide all currently showing UI
            foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
            {
                BallPrefab script = ballObj.GetComponent<BallPrefab>();
                script.SetSelected(false);
                //prevent any UI from showing
                script.allowUI = false;
                // Hide +- btns
                if(script.isCueBall == true)
                {
                    script.degreeBtnHolder.SetActive(false);
                }
            }
        }
        else
        {
            //allow UI to show
            foreach (GameObject ballObj in GameObject.FindGameObjectsWithTag("Ball_Marker"))
            {
                ballObj.GetComponent<BallPrefab>().allowUI = true;
                // Show +- btns
                if (ballObj.GetComponent<BallPrefab>().isCueBall == true)
                {
                    ballObj.GetComponent<BallPrefab>().degreeBtnHolder.SetActive(true);
                }
            }
        }
    }
}
