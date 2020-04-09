/// File: CamController.cs
/// Project: Open Tours VR - Model Config Tool
/// Programmers: The Weeping Angels
/// First Version: April 5th, 2020
/// Description: This file controls the camera's movement based on WADS controls

using UnityEngine;

public class CamController : MonoBehaviour
{
    /// <summary>
    /// The Model Object
    /// </summary>
    public GameObject Model;

    /// <summary>
    /// Updates Camera position based on WADS input
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // Zoom cam in
            transform.position += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            // Zoom cam out
            transform.position -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // Rotate left around model
            RotateCamera(CamDirections.Left);
        }
        if (Input.GetKey(KeyCode.D))
        {
            // Rotate right around model
            RotateCamera(CamDirections.Right);
        }
    }

    /// <summary>
    /// Rotates the main camera in a direction around the model
    /// </summary>
    /// <param name="dir"></param>
    private void RotateCamera(CamDirections dir)
    {
        transform.RotateAround(Model.transform.position, new Vector3(0, (int)dir, 0), 1);
        transform.LookAt(Model.transform.position);
    }

    /// <summary>
    /// Enum of cam direction values
    /// </summary>
    private enum CamDirections
    {
        Left = 1,
        Right = -1
    }
}
