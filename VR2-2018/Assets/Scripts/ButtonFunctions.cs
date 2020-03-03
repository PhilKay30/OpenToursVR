using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{

    public GameObject plane;
    public void Toggle()
    {
        

        if (plane.activeSelf == true)
        {
            plane.SetActive(false);
            Debug.Log("Setting Plane to false");
        }
        else
        {
            plane.SetActive(true);
            Debug.Log("Setting Plane to true");
        }
    }
}
