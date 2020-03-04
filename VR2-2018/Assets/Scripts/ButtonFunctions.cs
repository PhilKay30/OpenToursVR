using UnityEngine;
using UnityEngine.UI;
using Valve.VR;


public class ButtonFunctions : MonoBehaviour
{

    public GameObject historicalPlane;
    public GameObject menuCanvas;
    public SteamVR_Input_Sources leftHand;
    public SteamVR_Input_Sources rightHand;

    public void Update()
    {
        // Left Joystick Push down
        if (SteamVR_Input.GetStateDown("LeftJoystickButton", leftHand))
        {
            Toggle(menuCanvas);
        }
    }
    public void Toggle(GameObject plane)
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
