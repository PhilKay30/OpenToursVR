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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class GenerateWorld : MonoBehaviour
{
    // Unity Objects
    private Texture2D teleportMap;
    private Texture osmMap;
    private Texture historyMap;

    private API_Handler api = new API_Handler();
    private byte[] osmMapData;
    private byte[] historyMapData; // to hold future byte array from Db

    // Eventually get these from Db
    private string osmMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\output.png";
    private string teleportMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\32x32Marked2.png";
    private string historyMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\historyMap.png";
    private List<Dictionary<string, double>> mapBounds = new List<Dictionary<string, double>>();            /// Will hold the top left and bottom right points
    private List<Dictionary<string, double>> dataPointId = new List<Dictionary<string, double>>();          /// Will hold point_id, and point location
    private List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex

    private float scaler = 350.0f;  /// To translate pixel to scale in unity
                                    /// To hold the scale. 10 pixels per scale point
                                    /// For example a 32 pixel will need a scale of 3.2

    public GameObject teleportPoint;    /// The object that will hold the teleport point (a point of interest)
    public GameObject mapPlane;         /// The plane that will hold the map
    public GameObject teleportPlane;    /// The plane that will hold the teleport area
    public GameObject historyPlane;     /// The plane that will hold the historical map
    public GameObject bottomLayerPlane; /// The under plane to hide any transparency

    

    // Start is called before the first frame update
    void Start()
    {
        // API Calls 
        osmMapData = api.GetOsmMap(); //working
        mapBounds = api.GetMapBounds(); //working
        dataPointId = api.GetPointLocations(); // working
        
        
        
        osmMap = LoadDataIntoTexture(osmMapData);
        //osmMap = LoadMaps(osmMapFile);
        

        teleportMap = LoadMaps(teleportMapFile);
        historyMap = LoadMaps(historyMapFile);
        
        CreateMapPlane();
        PlaceHistoryMap();

        PlacePointsOfInterest();
    }




    private void PlacePointsOfInterest()
    {
        Dictionary<string, double> bottom_right = new Dictionary<string, double>();
        Dictionary<string, double> top_left = new Dictionary<string, double>();

        // This will be ugly and should be done better
        // Here there be magic
        bottom_right["longitude"] = mapBounds[0]["longitude"];
        bottom_right["latitude"] = mapBounds[0]["latitude"];
        top_left["longitude"] = mapBounds[1]["longitude"];
        top_left["latitude"] = mapBounds[1]["latitude"];

        //How the dataPointId is grabbing the data_points table
        /*
                index
                    |_id : the id in the Db
                    |_longitude : longitude associate with id
                    |_latitude  : latitude associate with id 
         */

        foreach(var entry in dataPointId)
        {
            // Make API call to get POI data
            dataPointInformation.Add(api.GetPointInformation(entry["id"]));

            /*   FANCY MATH HERE   */
            /*   END FANCY MATH    */

            // The next line will instantiate a teleport point at point x and y.  0 is for how high
            // off the plane it should be 
            //Instantiate(teleportPoint, new Vector3(posX, 0, PosY), Quaternion.identity);
        }

    }




    /// <summary>
    /// Create a Texture out of a byte array
    /// </summary>
    /// <param name="osmData">The Byte array</param>
    /// <returns>Texture</returns>
    public Texture2D LoadDataIntoTexture(byte[] osmData)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(osmData); //..this will auto-resize the texture dimensions.
        return tex;
    }



    /// <summary>
    /// LoadMaps
    /// Description  :    This will load the map files into memory 
    /// </summary>
    /// <param name="mapFile">The path of the file</param>
    /// <returns>Texture2D</returns>
    public Texture2D LoadMaps(string mapFile)
    {
        return LoadPNG(mapFile);
    }





    /// <summary>
    /// LoadPNG
    /// Description  :    This will take an image file and load it into a Texture object
    ///                   Used this post to help out
    ///                   https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
    /// </summary>
    /// <param name="filePath">the path of the file</param>
    /// <returns>Texture2D</returns>
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
        //float positionX = CalculatePosition(scaleX);
        //float positionZ = CalculatePosition(scaleZ);
        mapPlane.transform.position = new Vector3(scaleX, 0, scaleZ);
        bottomLayerPlane.transform.position = new Vector3(scaleX, -0.1f, scaleZ);
        bottomLayerPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);

        // Creates and holds the material to go on the plane
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = osmMap;

        mapPlane.GetComponent<Renderer>().material = material;

        // Create the Teleport Area
        teleportPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        teleportPlane.transform.position = new Vector3(scaleX, 0.1f, scaleZ);

    }



    private void PlaceHistoryMap()
    {
        if (historyMap == null)
        {
            historyPlane.SetActive(false);
            return;
        }
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







}
