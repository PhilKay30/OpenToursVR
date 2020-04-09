/// File: ButtonHandler.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file contains button handlers for the UI

using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    /// <summary>
    /// Fields to get and store the model controller and object loader
    /// </summary>
    public ModelController MyController;
    public ObjecLoader MyLoader;

    /// <summary>
    /// Starts scaling the model with a multiplier (OnButtonDown)
    /// </summary>
    /// <param name="multiplier"></param>
    public void StartScaling(float multiplier)
    {
        MyController.IsScaling = true;
        MyController.ScaleFactor = multiplier;
    }

    /// <summary>
    /// Stops the model from scaling (OnButtonUp)
    /// </summary>
    public void StopScaling()
    {
        MyController.IsScaling = false;
        MyController.ScaleFactor = 0f;
    }

    /// <summary>
    /// Starts rotating the model left (OnButtonDown)
    /// </summary>
    public void StartRotatingLeft()
    {
        MyController.Dir = RotateDirections.Left;
        MyController.IsRotating = true;
    }

    /// <summary>
    /// Starts rotating the model right (OnButtonDown)
    /// </summary>
    public void StartRotatingRight()
    {
        MyController.Dir = RotateDirections.Right;
        MyController.IsRotating = true;
    }

    /// <summary>
    /// Stops the model from rotating (OnButtonUp)
    /// </summary>
    public void StopRotating()
    {
        MyController.IsRotating = false;
    }

    /// <summary>
    /// Saves the Model (OnClick)
    /// </summary>
    public void SaveModel()
    {
        MyLoader.SaveModel();
    }

    /// <summary>
    /// Exits the application (OnClick)
    /// </summary>
    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
