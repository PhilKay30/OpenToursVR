/*
* Project			: 	Capstone
* File				: 	LaserInput.cs
* Programmer		: 	Wheeping Angels team
* First Version		: 	January 2020
* Description		:	This attachs a laser input to the hand and once the A button is pressed
*                       on the controller on a UI object in the Gamespace it will activate the button's 
*                       function
*                       
*                       Used this to help
*                       https://medium.com/@danielle.co3tz33/laser-pointer-with-ui-buttons-unity3d-for-vr-f0293022e489
*                       
*                       TODO: Change it to make it more generic to press any button
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using System;

public class LaserInput : MonoBehaviour
{
    public static GameObject currentObject;
    int currentId;
    public GameObject panel;
    //List<RaycastHit> hits = new List<RaycastHit>();
    public SteamVR_Input_Sources rightHand;

    // Start is called before the first frame update
    void Start()
    {
        currentObject = null;
        currentId = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        //hits.Clear();
        if (SteamVR_Input.GetStateDown("A", rightHand))
        {
            Debug.Log("Pressed A");
            RaycastHit[] hits;
            //hits.AddRange(Physics.RaycastAll(transform.position, transform.forward, 5.0f));
            hits = Physics.RaycastAll(transform.position, transform.forward, 1.5f);
            Debug.Log(hits.Length);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                int id = hit.collider.gameObject.GetInstanceID();

                if (currentId != id)
                {
                    currentId = id;
                    currentObject = hit.collider.gameObject;
                    string name = currentObject.name;

                    if (name == "PressMeBtn")
                    {
                        Debug.Log("Display trigger");
                        DisplayInfo();
                    }
                }
            }
        }
    }

    private void DisplayInfo()
    {
       
        if (panel.activeSelf == false)
        {
            Debug.Log("Setting active to True");
            panel.SetActive(true);
            currentId = 0;
        }
        else
        {
            Debug.Log("Setting active to False");
            panel.SetActive(false);
            currentId = 0;
        }
    }
}
