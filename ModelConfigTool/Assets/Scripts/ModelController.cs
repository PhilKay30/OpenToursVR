/// File: ModelController.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file updates the orientation of the model

using UnityEngine;

public class ModelController : MonoBehaviour
{
    /// <summary>
    /// The Model
    /// </summary>
    public GameObject Model;

    /// <summary>
    /// 
    /// </summary>
    public bool IsScaling = false;
    public float ScaleFactor = 0f;
    public bool IsRotating = false;
    public RotateDirections Dir = RotateDirections.Left;


    /// <summary>
    /// Updates the model position / rotation
    /// </summary>
    void Update()
    {
        if (IsScaling)
        {
            Vector3 currScale = Model.transform.localScale;
            Model.transform.localScale = new Vector3(currScale.x * ScaleFactor, currScale.y * ScaleFactor, currScale.z * ScaleFactor);
        }
        if (IsRotating)
        {
            Model.transform.Rotate(new Vector3(0, (int)Dir, 0), Space.Self);
        }
    }
}

/// <summary>
/// Enum of rotation directions for the model
/// </summary>
public enum RotateDirections
{
    Right = 1,
    Left = -1
}
