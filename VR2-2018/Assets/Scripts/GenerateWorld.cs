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
using System.Linq;
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
    //private string osmMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\output.png";
    //private string teleportMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\32x32Marked2.png";
    //private string historyMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\historyMap.png";
    
    private List<Dictionary<string, double>> mapBounds = new List<Dictionary<string, double>>();            /// Will hold the top left and bottom right points
    private List<Dictionary<string, double>> dataPointId = new List<Dictionary<string, double>>();          /// Will hold point_id, and point location
   

    private float scaler = 400.0f;  /// To translate pixel to scale in unity
                                    /// To hold the scale. 10 pixels per scale point
                                    /// For example a 32 pixel will need a scale of 3.2

    public GameObject teleportPoint;    /// The object that will hold the teleport point (a point of interest)
    public GameObject mapPlane;         /// The plane that will hold the map
    public GameObject teleportPlane;    /// The plane that will hold the teleport area
    public GameObject historyPlane;     /// The plane that will hold the historical map
    public GameObject bottomLayerPlane; /// The under plane to hide any transparency

    // This is the height the datapoints will be created at (accessible from IDE)
    public float HeightOfDataPoints = 0.05f;

    /// <summary>
    /// This object will conatin all of the historical map data
    /// </summary>
    HistMapObj histMapContainer = new HistMapObj();

    public static List<DataPointContainer> dpc = new List<DataPointContainer>(); /// This will hold a list of datapoint and their vector3s
    

    // Start is called before the first frame update
    void Start()
    {
        // API Calls 
        osmMapData = api.GetOsmMap(); //working
        mapBounds = api.GetMapBounds(); //working
        dataPointId = api.GetPointLocations(); // working
		try
		{
			histMapContainer = api.GetHistMap();
            historyMapData = histMapContainer.MapData;
            historyMap = LoadDataIntoTexture(historyMapData);
		}
		catch (Exception e)
		{
            Debug.Log("Historical map not found: " + e);
			// This means there was no historical map in the db
		}
        

        osmMap = LoadDataIntoTexture(osmMapData);
        
        
        CreateMapPlane();
        PlacePointsOfInterest();

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

        
    }


    void Update()
    {
       
    }

   



    /// <summary>
    /// These hold the width and height of osm map in unity pixels
    /// </summary>
    double mapPixelWidth = 0;
    double mapPixelLength = 0;

    /// <summary>
    /// These will hold the map boundaries
    /// </summary>
    Dictionary<string, double> bottom_right = new Dictionary<string, double>();
    Dictionary<string, double> top_left = new Dictionary<string, double>();

    // This will give ratios between the unity coord system and the GIS coord system (pixels per GIS)
    double pixelsToGISRatioWidth = 0;
    double pixelsToGISRatioLength = 0;

    /// <summary>
    /// This will be the bottom left point in unity coords of base map
    /// </summary>
    Vector3 botLeftMapLocation = new Vector3();




    private void PlacePointsOfInterest()
    {
        // This will be ugly and should be done better
        // Here there be magic
        bottom_right["longitude"] = mapBounds[0]["longitude"];
        bottom_right["latitude"] = mapBounds[0]["latitude"];
        top_left["longitude"] = mapBounds[1]["longitude"];
        top_left["latitude"] = mapBounds[1]["latitude"];



        // This will give current width of osmMap (10 is base size of plane)
        mapPixelWidth = 10f * (double)mapPlane.transform.localScale.x;
        // This will give current length of osmMap (10 is base size of plane)
        mapPixelLength = 10f * (double)mapPlane.transform.localScale.z;
        // This will give the width of the osmMap in GIS value
        double mapGISWidth = bottom_right["longitude"] - top_left["longitude"];
        // This will give the length of the osmMap in GIS value
        double mapGISLength = top_left["latitude"] - bottom_right["latitude"];

        // This will give ratios between the unity coord system and the GIS coord system (pixels per GIS)
        pixelsToGISRatioWidth = mapPixelWidth / mapGISWidth;
        pixelsToGISRatioLength = mapPixelLength / mapGISLength;

        // Now get the bottom left corner of the map (in unity coords)
        Vector3 mapLocation = mapPlane.transform.position;
        botLeftMapLocation.x = (float)(mapLocation.x - (mapPixelWidth / 2));
        botLeftMapLocation.y = mapLocation.y;
        botLeftMapLocation.z = (float)(mapLocation.z - (mapPixelLength / 2));

       
        foreach (var entry in dataPointId)
        {
            double dataPointLatitude = entry["latitude"];
            double dataPointLongitude = entry["longitude"];
            Vector2 unityPos = GisToUnity(new Vector2((float)dataPointLongitude, (float)dataPointLatitude));
            Vector3 dataPointUnityCoord = new Vector3(unityPos.x, HeightOfDataPoints, unityPos.y);


            DataPointContainer dp = new DataPointContainer();
            dp.Id = entry["id"];
            dp.PoiLocation = dataPointUnityCoord;
            dpc.Add(dp);
            Instantiate(teleportPoint, dataPointUnityCoord, Quaternion.identity);
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
        //mapPlane.transform.position = new Vector3(scaleX, mapPlane.transform.position.y, scaleZ);
        //bottomLayerPlane.transform.position = new Vector3(scaleX, bottomLayerPlane.transform.position.y, scaleZ);
        bottomLayerPlane.transform.localScale = new Vector3(scaleX, bottomLayerPlane.transform.localScale.y, scaleZ);

        // Creates and holds the material to go on the plane
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.renderQueue = 2998; //to fix the clipping issue
        material.mainTexture = osmMap;

        mapPlane.GetComponent<Renderer>().material = material;

        // Create the Teleport Area
        teleportPlane.transform.localScale = new Vector3(scaleX, teleportPlane.transform.localScale.y, scaleZ);
        //teleportPlane.transform.position = new Vector3(scaleX, teleportPlane.transform.position.y, scaleZ);
    }


    /// <summary>
    /// This method places the historical map in the correct location
    /// </summary>
    private void PlaceHistoryMap()
    {
        // get km dimensions of OSM map and historical map
        Vector2 OsmDimensionsKM = api.OsmMapDimensions;
        Vector2 HistMapDimensionsKM = new Vector2(histMapContainer.WidthKM, histMapContainer.HeightKM);
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
        historyPlane.transform.Rotate(0, histMapContainer.Rotation, 0, Space.Self);
        
        // get the location that the historical map is supposed to be in and then move it there
        Vector2 unityPos = GisToUnity(histMapContainer.CenterPoint);
        historyPlane.transform.position = new Vector3(unityPos.x, historyPlane.transform.position.y, unityPos.y);
      
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = historyMap;
        material.renderQueue = 2999; //to fix the clipping issue
        historyPlane.GetComponent<Renderer>().material = material;
    }




    /// <summary>
    /// Translates a GIS point to a Unity position
    /// </summary>
    /// <param name="gisCoords"></param>
    /// <returns></returns>
    private Vector2 GisToUnity(Vector2 gisCoords)
    {
        /*   FANCY MATH HERE   */
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





    /// <summary>
    /// Here be a possible Obsolete method, should be safe for deletion
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
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




/// <summary>
/// This will hold the contants of a point of interest
/// </summary>
public class DataPointContainer
{
    public double Id { set; get; }
    public Vector3 PoiLocation { set; get; }

    public DataPointContainer()
    {
        Id = 0;
        PoiLocation = new Vector3();
    }
}
