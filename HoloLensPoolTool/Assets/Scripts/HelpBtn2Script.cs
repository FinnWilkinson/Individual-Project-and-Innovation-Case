using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Boundary;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class HelpBtn2Script : MonoBehaviour
{
    public GameObject helpBtn;
    public GameObject helpBox;

    // Start is called before the first frame update
    void Start()
    {
        helpBox.SetActive(false);

        // Set listener for when help btn is pressed
        helpBtn.GetComponent<Interactable>().OnClick.AddListener(() => toggleHelp());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void toggleHelp()
    {
        if (helpBtn.GetComponent<Interactable>().IsToggled)
        {
            // If toggled, show help text box 
            helpBox.SetActive(true);
        }
        else
        {
            // Hide help text box 
            helpBox.SetActive(false);
        }
    }
}
