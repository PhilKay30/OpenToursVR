/// File: SliderController.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: March 20th, 2020
/// Description: This file contains button handlers for the UI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public GameObject OverlayMap;

    /// <summary>
    /// UPDATE method for Unity
    /// Updates the rotation of the hist map
    /// </summary>
    public void UpdateRotation()
    {
        OverlayMap.transform.rotation = Quaternion.Euler(0, slider.value * 360, 0);
    }
}
