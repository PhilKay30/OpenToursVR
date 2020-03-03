/*
* Project			: 	Capstone
* File				: 	GenerateWorld.cs
* Programmer		: 	Wheeping Angels team
* First Version		: 	January 2020
* Description		:	This script will generate the world assets needed to run the world
*                       It will generate a plane and then load a map image upon it 
*                       It will generate SteamVR Teleport points based on a tile map
*/

using System;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class GenerateWorld : MonoBehaviour
{
    // Unity Objects
    private Texture2D teleportMap;
    private Texture osmMap;
    private Texture historyMap;
    public GameObject mapPlane; // The plane that will hold the map
    public GameObject teleportPlane; // The plane that will hold the teleport area
    public GameObject historyPlane;

    // Eventually get these from Db
    //private string osmMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\1ksq_v2.png";
    private string teleportMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\32x32Marked.png";
    private string historyMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\historyMap.png";
    
    
    private float scaler = 350.0f; // To translate pixel to scale in unity
                                 // To hold the scale. 10 pixels per scale point
                                 // For example a 32 pixel will need a scale of 3.2

    private API_Handler api = new API_Handler();
    byte[] osmData;

    // Start is called before the first frame update
    void Start()
    {
        osmData = api.GetOsmMap();
        teleportMap = LoadMaps(teleportMapFile);
        osmMap = LoadData(osmData);
        //osmMap = LoadMaps(osmMapFile);
        historyMap = LoadMaps(historyMapFile);
        CreateMapPlane();
        PlaceHistoryMap();
    }

    private Texture LoadData(byte[] osmData)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(osmData); //..this will auto-resize the texture dimensions.
        return tex;
    }

    private void PlaceHistoryMap()
    {
        float hMapWidth = osmMap.width;
        float hMapHeight = osmMap.height;
        float scaleX = historyMap.width / scaler;
        float scaleZ = historyMap.height / scaler;
        float positionX = CalculatePosition(scaleX);
        float positionZ = CalculatePosition(scaleZ);

        historyPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        historyPlane.transform.position = new Vector3(positionX * 2, 0.05f, positionZ * 2);
        // Creates and holds the material to go on the plane
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = historyMap;
        historyPlane.GetComponent<Renderer>().material = material;
    }


    /* CUSTOM Functions */

    /*
    * Function     :    LoadMaps
    * Description  :    This will load the map files into memory
    * Parameters   :    string mapFile: THe path of the file
    * Return Value :    Texture2D	
    */
    public Texture2D LoadMaps(string mapFile)
    {
        return LoadPNG(mapFile);
    }


    /*
    * Function     :    CreateMapPlane
    * Description  :    This Set the size of the plane and load the map as a texture on the plane
    * Parameters   :    None
    * Return Value :    None	
    */
    private void CreateMapPlane()
    {
        float scaleX = osmMap.width / scaler;
        float scaleZ = osmMap.height / scaler;


        // Creates the plane
        mapPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        float positionX = CalculatePosition(scaleX);
        float positionZ = CalculatePosition(scaleZ);
        mapPlane.transform.position = new Vector3(positionX, 0, positionZ);

        // Creates and holds the material to go on the plane
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = osmMap;

        mapPlane.GetComponent<Renderer>().material = material;

        // Create the Teleport Area
        teleportPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        teleportPlane.transform.position = new Vector3(positionX, 0.1f, positionZ);

    }





    /*
    * Function     :    calculatePosition	
    * Description  :    This is to move the plane Lower left corner to the 0,0 position
    *                   based on the plane's X and Z scales
    * Parameters   :    float scale: The scale of axis
    * Return Value :    float: The position the plane needs to be 	
    */
    private float CalculatePosition(float scale)
    {
        // scale 1 positions
        float baseScale = 1f;           // Starting scale for any GameObject is 1
        float basePosition = 4.5f;      // A plane at scale one needs to be set at 4.5 X, Z position

        float multiplier = 0.5f * 10f;  // Every 0.1 increment in scales means a 0.5 movement.
                                        
        // Calculate how far from baseScale the number
        // multiply that number but the multiplier and then add the basePosition
        // ie.  Scale of 1.4
        // 1.4 - 1 = 0.4
        // 0.4 * 0.5 * 10 = 2
        // 2 + 4.5 = 6.5  So the new position for the plane will be 6.5
        float position = (scale - baseScale) * multiplier + basePosition;
        return position;
    }







    /*
    * Function     :	LoadPNG
    * Description  :    This will take an image file and load it into a Texture object
    *                   Used this post to help out
    *                   https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
    * Parameters   :    string filePath: The file path of the image
    * Return Value :    Texture2D: The image loaded into a Texture2D
    */
    // 
    public static Texture2D LoadPNG(string filePath)
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
