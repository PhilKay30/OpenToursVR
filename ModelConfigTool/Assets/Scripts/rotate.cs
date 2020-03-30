/// File: rotate.cs
/// Project: Open Tours VR - Model Config Tool
/// Programmers: The Weeping Angels
/// First Version: March 25th, 2020
/// Description: This file controls the manipulation of the model object's size, rotation, and height

using Dummiesman;
using System.IO;
using UnityEngine;

public class rotate : MonoBehaviour
{
    /// <summary>
    /// The Model
    /// </summary>
    public GameObject Model;

    /// <summary>
    /// Booleans for keeping track of whether a UI button is being held down to scale the object up/down
    /// </summary>
    private bool IsScalingUp = false;
    private bool IsScalingDown = false;


    /// <summary>
    /// Update method for Unity MonoBehaviour
    /// Handles the scaling, rotation, and positioning of the obj based on keyboard / booleans
    /// </summary>
    void Update()
    {
        // Check if any actions need to be performed
        if (Input.GetKey("up")) // if UP key is currently own
        {
            MoveUp();
        }
        if (Input.GetKey("down")) // if down key is currently down
        {
            MoveDown();
        }
        if (Input.GetKey("left")) // if left key is currently down
        {
            RotateLeft();
        }
        if (Input.GetKey("right")) // if right key is currently down
        {
            RotateRight();
        }
        if (IsScalingUp) // if user is choosing to enlarge the obj
        {
            ModifyScale(1.1f);
        }
        if (IsScalingDown) // if user is choosing to shrink the obj
        {
            ModifyScale(0.9f);
        }
    } // END Update Method for Unity MonoBehaviour


    /// <summary>
    /// Move the object up.
    /// Call this method once to move obj up by 0.01f.
    /// </summary>
    private void MoveUp()
    {
        var curr = Model.transform.position;
        Model.transform.position = new Vector3(curr.x, curr.y + 0.01f, curr.z);
    }

    /// <summary>
    /// Move the object down
    /// Call this method once to move the obj down by 0.01f
    /// </summary>
    private void MoveDown()
    {
        var curr = Model.transform.position;
        Model.transform.position = new Vector3(curr.x, curr.y - 0.01f, curr.z);
    }

    /// <summary>
    /// Modify the scale of the object with a multiplier
    /// Call this method once to change the scale of the object once
    /// </summary>
    /// <param name="multiplier"></param>
    private void ModifyScale(float multiplier)
    {
        Vector3 currScale = Model.transform.localScale;
        Model.transform.localScale = new Vector3(currScale.x * multiplier, currScale.y * multiplier, currScale.z * multiplier);
    }

    /// <summary>
    /// Rotate the obj right.
    /// Call this method once to rotate the obj by 1 degree.
    /// </summary>
    private void RotateRight()
    {
        Model.transform.Rotate(new Vector3(0, -1, 0), Space.Self);
    }

    /// <summary>
    /// Rotate the obj left.
    /// Call this method once to rotate the obj by 1 degree.
    /// </summary>
    private void RotateLeft()
    {
        Model.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
    }
       

    /// <summary>
    /// Button hook to start enlarging obj.
    /// Used when button is clicked down.
    /// </summary>
    public void StartScaleUp()
    {
        IsScalingUp = true;
    }

    /// <summary>
    /// Button hook to stop enlarging obj.
    /// Used when button is released.
    /// </summary>
    public void StopScaleUp()
    {
        IsScalingUp = false;
    }

    /// <summary>
    /// Button hook to start shrinking obj.
    /// Used when button is clicked down.
    /// </summary>
    public void StartScaleDown()
    {
        IsScalingDown = true;
    }

    /// <summary>
    /// Button hook to stop shrinking obj.
    /// Used when button is released.
    /// </summary>
    public void StopScaleDown()
    {
        IsScalingDown = false;
    }
}
