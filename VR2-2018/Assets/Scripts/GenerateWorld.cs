/*
* Project			: 	Capstone
* File				: 	GenerateWorld.cs
* Programmer		: 	Wheeping Angels team
* First Version		: 	January 2020
* Description		:	This script will generate the world assets needed to run the world
*                       It will generate a plane and then load a map image upon it 
*                       It will generate SteamVR Teleport points based on a tile map
*/

using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class GenerateWorld : MonoBehaviour
{
    // Unity Objects
    private Texture osmMap;
<<<<<<< HEAD
    
=======
    //private Texture historyMap;

    //private API_Handler api = new API_Handler();
    private byte[] osmMapData;
    //private byte[] historyMapData; 

    
    //private List<Dictionary<string, double>> mapBounds = new List<Dictionary<string, double>>();            /// Will hold the top left and bottom right points
    //private List<Dictionary<string, double>> dataPointId = new List<Dictionary<string, double>>();          /// Will hold point_id, and point location
   

>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    private float scaler = 400.0f;  /// To translate pixel to scale in unity
                                    /// To hold the scale. 10 pixels per scale point
                                    /// For example a 32 pixel will need a scale of 3.2

    /// <summary>
    /// These hold the width and height of osm map in unity pixels
    /// </summary>
    private double mapPixelWidth = 0;
    private double mapPixelLength = 0;

    /// <summary>
    /// These will hold the map boundaries
    /// </summary>
    private Dictionary<string, double> bottom_right = new Dictionary<string, double>();
    private Dictionary<string, double> top_left = new Dictionary<string, double>();

    // This will give ratios between the unity coord system and the GIS coord system (pixels per GIS)
    private double pixelsToGISRatioWidth = 0;
    private double pixelsToGISRatioLength = 0;

    /// <summary>
    /// This will be the bottom left point in unity coords of base map
    /// </summary>
    private Vector3 botLeftMapLocation = new Vector3();



    public GameObject teleportPoint;    /// The object that will hold the teleport point (a point of interest)
    public GameObject mapPlane;         /// The plane that will hold the map
    public GameObject teleportPlane;    /// The plane that will hold the teleport area
    public GameObject historyPlane;     /// The plane that will hold the historical map
    public GameObject bottomLayerPlane; /// The under plane to hide any transparency

    // This is the height the datapoints will be created at (accessible from IDE)
    private float HeightOfDataPoints = 0.0f;


<<<<<<< HEAD

=======
    /// <summary>
    /// This object will conatin all of the historical map data
    /// </summary>
    //HistMapObj histMapContainer = new HistMapObj();

    
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    

    // Start is called before the first frame update
    void Start()
    {
<<<<<<< HEAD
        StartCoroutine(CreatePlanes());
        StartCoroutine(LoadModels());
=======
        //APICalls();
        CreatePlanes();
        LoadModels();
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    }




    /// <summary>
    /// IN this function we will create the plane from the 
    /// data we've received from the API calls
    /// </summary>
<<<<<<< HEAD
    private IEnumerator CreatePlanes()
=======
    private void CreatePlanes()
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    {
        osmMap = LoadDataIntoTexture(API_Data_Loader.osmMapData);
        CreateMapPlane();
        StartCoroutine(PlacePointsOfInterest());

        try
        {
            PlaceHistoryMap();
        }
        catch (Exception e)
        {
            Debug.Log("PlaceHistoryMap:" + e);
            // this means there was no historical map in the object returned from db
            // this catch is just here so everything else continues
            // if we want to disable the histPlane, it can be done here
        }


        mapPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
        historyPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
<<<<<<< HEAD
        yield return new WaitForEndOfFrame();
=======
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    }






    /// <summary>
    /// This method loads and orients all models
    /// </summary>
    private IEnumerator LoadModels()
    {
<<<<<<< HEAD
=======
        //List<ModelHandle> models = api.GetModels();
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
        foreach (ModelHandle m in API_Data_Loader.models)
        {
            m.GameObj = new OBJLoader().Load(m.FilePath);
            m.GameObj.transform.localScale = m.Scale;
            m.GameObj.transform.rotation = m.Rotation;
            Vector3 pos = GisToUnity(m.Position);
<<<<<<< HEAD
=======
            //pos.y = m.Offset + mapPlane.transform.position.y;
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
            pos.y = m.Offset + 0.1f;
            m.GameObj.transform.position = pos;
            yield return new WaitForEndOfFrame();
        }
    }

   




    private IEnumerator PlacePointsOfInterest()
    {
<<<<<<< HEAD
=======
        // This will be ugly and should be done better
        // Here there be magic
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
        bottom_right["longitude"] = API_Data_Loader.mapBounds[0]["longitude"];
        bottom_right["latitude"] = API_Data_Loader.mapBounds[0]["latitude"];
        top_left["longitude"] = API_Data_Loader.mapBounds[1]["longitude"];
        top_left["latitude"] = API_Data_Loader.mapBounds[1]["latitude"];



        // This will give current width/length of osmMap (10 is base size of plane)
        mapPixelWidth = 10f * (double)mapPlane.transform.localScale.x;
        mapPixelLength = 10f * (double)mapPlane.transform.localScale.z;
        
        // This will give the width/length of the osmMap in GIS value
        double mapGISWidth = bottom_right["longitude"] - top_left["longitude"];
        double mapGISLength = top_left["latitude"] - bottom_right["latitude"];

        // This will give ratios between the unity coord system and the GIS coord system (pixels per GIS)
        pixelsToGISRatioWidth = mapPixelWidth / mapGISWidth;
        pixelsToGISRatioLength = mapPixelLength / mapGISLength;


        // Now get the bottom left corner of the map (in unity coords)
        Vector3 mapLocation = mapPlane.transform.position;
        botLeftMapLocation.x = (float)(mapLocation.x - (mapPixelWidth / 2));
        botLeftMapLocation.y = mapLocation.y;
        botLeftMapLocation.z = (float)(mapLocation.z - (mapPixelLength / 2));

       
        foreach (var entry in API_Data_Loader.dpInfo)
        {
            float dataPointLatitude = (float)entry.Latitude;
            float dataPointLongitude = (float)entry.Longitude;
            Vector2 unityPos = GisToUnity(new Vector2(dataPointLongitude, dataPointLatitude));
            Vector3 dataPointUnityCoord = new Vector3(unityPos.x, HeightOfDataPoints, unityPos.y);
            Instantiate(teleportPoint, dataPointUnityCoord, Quaternion.identity);
            entry.PoiLocation = dataPointUnityCoord;
<<<<<<< HEAD
            yield return new WaitForEndOfFrame();
=======
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
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
        bottomLayerPlane.transform.localScale = new Vector3(scaleX, bottomLayerPlane.transform.localScale.y, scaleZ);

        // Creates and holds the material to go on the plane
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.renderQueue = 2998; //to fix the clipping issue
        material.mainTexture = osmMap;

        mapPlane.GetComponent<Renderer>().material = material;

        // Create the Teleport Area
        teleportPlane.transform.localScale = new Vector3(scaleX, teleportPlane.transform.localScale.y, scaleZ);
    }


    /// <summary>
    /// This method places the historical map in the correct location
    /// </summary>
    private void PlaceHistoryMap()
    {
        // get km dimensions of OSM map and historical map
        Vector2 OsmDimensionsKM = API_Data_Loader.api.OsmMapDimensions;
        Vector2 HistMapDimensionsKM = new Vector2(API_Data_Loader.histMapContainer.WidthKM, API_Data_Loader.histMapContainer.HeightKM);
<<<<<<< HEAD
 
=======
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
        // get ratios between the 2 map sizes
        float horizontalRatio = HistMapDimensionsKM.x / OsmDimensionsKM.x;
        float verticalRatio = HistMapDimensionsKM.y / OsmDimensionsKM.y;
        
        // apply the ratio to the OsmPlane size to get the size of historical map in unity size
        float histMapWidthPxl = horizontalRatio * (float)mapPixelWidth;
        float histMapLengthPxl = verticalRatio * (float)mapPixelLength;

        // get values to scale the hist plane (plane objects are normally 10x10 pixels).
        // these calculations are derived from the fact that base size of plane is 10 pixels
        // thus i took equation: (10 * scaler = numOfPixels) and rearraged into (scaler = numOfPixels / 10).

        float horizontalScaler = histMapWidthPxl / 10 ;
        float verticalScaler = histMapLengthPxl / 10;
        
        // scale the plane correctly
        historyPlane.transform.localScale = new Vector3(horizontalScaler, historyPlane.transform.localScale.y, verticalScaler);
        
        // rotate the hist plane by correct amount (pretty sure this will do what I want)
        historyPlane.transform.Rotate(0, API_Data_Loader.histMapContainer.Rotation, 0, Space.Self);
        
        // get the location that the historical map is supposed to be in and then move it there
        Vector2 unityPos = GisToUnity(API_Data_Loader.histMapContainer.CenterPoint);
        historyPlane.transform.position = new Vector3(unityPos.x, historyPlane.transform.position.y, unityPos.y);
      
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = API_Data_Loader.historyMap;
        material.renderQueue = 2999; //to fix the clipping issue
        historyPlane.GetComponent<Renderer>().material = material;
    }




    /// <summary>
    /// Translates a GIS point to a Unity position
    /// </summary>
    /// <param name="gisCoords"></param>
    /// <returns></returns>
    public Vector2 GisToUnity(Vector2 gisCoords)
    {
        //   FANCY MATH HERE   
        double distanceToMovePointLogitude = gisCoords.x - top_left["longitude"];
        double distanceToMovePointLatitude = gisCoords.y - bottom_right["latitude"];

        // Distance to move the data point (starting at bottom left of map in unity coords)
        double distanceToMoveInUnityX = distanceToMovePointLogitude * pixelsToGISRatioWidth;
        double distanceToMoveInUnityZ = distanceToMovePointLatitude * pixelsToGISRatioLength;

        // x and z coords (in unity) for the data point
        float dataPointX = (float)(distanceToMoveInUnityX + botLeftMapLocation.x);
        float dataPointZ = (float)(distanceToMoveInUnityZ + botLeftMapLocation.z);

        // This is the position the datapoint needs to be placed at (in unity coord system)
        // NOTE: The y value is a public property you can fiddel with in the IDE at runtime
        return new Vector2(dataPointX, dataPointZ);
    }
}