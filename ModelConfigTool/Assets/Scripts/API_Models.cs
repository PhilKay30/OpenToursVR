/// File: API_Models.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file contains Models for the API handler

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This object represents a Model
/// </summary>
[System.Serializable]
public class ModelObj
{
    public string model_location;
    public string model_rotation;
    public string model_scaling;
    public string model_data;
    public float model_offset;
}
