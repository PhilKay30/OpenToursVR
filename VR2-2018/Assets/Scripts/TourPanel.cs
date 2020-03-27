using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TourPanel : MonoBehaviour
{
    private API_Handler api = new API_Handler();
    private GenerateWorld gWorld = new GenerateWorld();
    
    private string openingText = "Welcome to OpenToursVR " +
        "To navigate look in the direction you wish to go push " +
        "forward on the left analog stick and let go. To make " +
        "me disapear press down on the left analog stick";

    public TextMeshProUGUI tourInfoText;
    public Image tourImage;
    public GameObject player;

   
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
        /// Here we get the player's position
        /// It changes every time with a variance of about 1.2
        /// The idea is to compare the player's X position to the 
        /// POI's X position.  But the variance will cause issues
        Vector3 playerPos = player.transform.position;


        foreach (var item in GenerateWorld.dpc)
        {
            Debug.Log("Player x: " + playerPos.x);
            Debug.Log("POI x: " + item.PoiLocation.x);
            Debug.Log("Difference: " + (item.PoiLocation.x - playerPos.x));

            if (CheckPosition(playerPos.x, item.PoiLocation.x))
            {
                List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex

                dataPointInformation.Add(api.GetPointInformation(item.Id));
                tourInfoText.text = dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"];
                Material mat = new Material(Shader.Find("Transparent/Diffuse"));
                Texture2D tex = gWorld.LoadDataIntoTexture(api.HexStringToBinary(dataPointInformation[0]["point_image"]));
                mat.mainTexture = tex;
                tourImage.material = mat;
            }
        }
    }


    /// <summary>
    /// Floating point math, great
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="poiLocation"></param>
    /// <returns></returns>
    private bool CheckPosition(float playerPos, float poiLocation)
    {
        float tollerance = 1.15f;
        float difference = poiLocation - tollerance;

        // https://stackoverflow.com/questions/3188672/how-to-elegantly-check-if-a-number-is-within-a-range
        // but that work only for Int, thanks for nothing LINQ
        // https://stackoverflow.com/questions/42906439/check-if-a-value-exists-between-two-numbers-float-c-sharp
        if (playerPos >= difference && playerPos <= poiLocation)
        {
            return true;
        }
        else
        {
            return false;
        }

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
