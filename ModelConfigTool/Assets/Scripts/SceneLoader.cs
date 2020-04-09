/// File: SceneLoader.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file contains an automatic scene loader for a loading screen to run.
/// 
/// REFERENCE: This code was modified by code found at:
///            https://blog.teamtreehouse.com/make-loading-screen-unity

using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// keep tack of whther loading has started (so it doesn't get called every Update)
    /// </summary>
    private bool isLoading = false;

    /// <summary>
    /// UPDATE
    /// Starts new scene loading if it hasn't done it already
    /// </summary>
    void Update()
    {
        if (isLoading == false)
        {
            isLoading = true;
            StartCoroutine(LoadNewScene());
        }
    }

    /// <summary>
    /// Loads a Scene asyncronously
    /// returns enumerable null's until the scene has loaded
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadNewScene()
    {
        yield return new WaitForSeconds(3);
        AsyncOperation async = Application.LoadLevelAsync("MainScene");
        while (!async.isDone)
        {
            yield return null;
        }
    }
}
