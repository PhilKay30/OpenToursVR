using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TourPanel : MonoBehaviour
{
    public GenerateWorld g;
    private string openingText = "Welcome to OpenToursVR " +
        "To navigate look in the direction you wish to go push " +
        "forward on the left analog stick and let go. To make " +
        "me disapear press down on the left analog stick";

    public TextMeshProUGUI tourInfoText;
    public Image tourImage;

   
    // Start is called before the first frame update
    void Start()
    {
        tourInfoText.text = openingText;
        
        //https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        Texture2D texture = LoadPNG(Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\default.jpg");
        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.mainTexture = texture;
        tourImage.material = mat;
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }





    public Texture2D LoadPNG(string filePath)
    {
        Debug.Log("Dir: " + Directory.GetCurrentDirectory().ToString());
        Debug.Log("filePath: " + filePath);
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            Debug.Log("File exists");
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}
