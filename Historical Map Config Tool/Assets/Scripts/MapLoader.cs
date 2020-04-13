using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

public class MapLoader : MonoBehaviour
{
    /// <summary>
    /// Plane objects to be set in Unity Designer
    /// </summary>
    public GameObject BaseMap;
    public GameObject OverlayMap;

    /// <summary>
    /// API calls (should be replaced by reading the config file of the WPF application... TJ?)
    /// </summary>
    private string getMapRequest = "";
    private string getBoundsRequest = "";
    private string addMapRequest = "";

    /// <summary>
    /// GIS points for four corner of the base map
    /// </summary>
    private Vector2 GISTopLeftCorner { get; set; }
    private Vector2 GISTopRightCorner { get; set; }
    private Vector2 GISBotLeftCorner { get; set; }
    private Vector2 GISBotRightCorner { get; set; }

    /// <summary>
    /// Ratio of width and height of basemap, used to stretch the base map to fit the PNG's width->height ratio
    /// </summary>
    private float BaseMapRatio { get; set; }

    /// <summary>
    /// public slider sontrol to be set in Unity designer
    /// used to get the current rotation to send to API apon confirmation
    /// </summary>
    public Slider slider;


    /// <summary>
    /// This method loads the 2 map files into into the application
    /// </summary>
    void Start()
    {
        try
        {
            string baseURL = new ConfigReader().GetApiURL();
            getMapRequest = baseURL + "/getimg/osmMap.png";
            getBoundsRequest = baseURL + "/getbounds/osmMap";
            addMapRequest = baseURL + "/addimg/";
            RetrieveMapCoords();
            PlaceBaseMap();
            PlaceOverlayMap();
        }
        catch (Exception e)
        {
            if (!File.Exists("myLog.txt"))
            {
                File.Create("myLog.txt");
            }
            List<string> lines = new List<string>();
            lines.Add(e.ToString());
            File.WriteAllLines("myLog.txt", lines.ToArray());
        }
    }


    /// <summary>
    /// This method will be called from a button click to save the historical map / historical map data to the DB
    /// </summary>
    public void SaveMap()
    {
        // Unity coords for BaseMap
        Vector2 basemapPosition = new Vector2(BaseMap.transform.position.x, BaseMap.transform.position.z);
        Vector2 unityTopLeftCorner = new Vector2(basemapPosition.x - 5, basemapPosition.y + (5 * BaseMap.transform.localScale.z));
        Vector2 unityTopRightCorner = new Vector2(basemapPosition.x + 5, basemapPosition.y + (5 * BaseMap.transform.localScale.z));
        Vector2 unityBotLeftCorner = new Vector2(basemapPosition.x - 5, basemapPosition.y - (5 * BaseMap.transform.localScale.z));
        Vector2 unityBotRightCorner = new Vector2(basemapPosition.x + 5, basemapPosition.y - (5 * BaseMap.transform.localScale.z));

        // Unity coord for center of OverlayMap
        Vector2 unityCenterPoint = new Vector2(OverlayMap.transform.position.x, OverlayMap.transform.position.z);

        // Basemap GIS info
        double baseMapWidthInGIS = GISTopRightCorner.x - GISTopLeftCorner.x;
        double baseMapWidthInUnity = unityTopRightCorner.x - unityTopLeftCorner.x;
        double baseMapHeightInGIS = GISTopLeftCorner.y - GISBotLeftCorner.y;
        double baseMapHeightInUnity = unityTopLeftCorner.y - unityBotLeftCorner.y;

        // ratio for basemap calculation
        double ratioX = baseMapWidthInGIS / baseMapWidthInUnity;
        double ratioY = baseMapHeightInGIS / baseMapHeightInUnity;

        // get unity coord offsets for overlay map
        double overlayMapOffsetX = unityCenterPoint.x - unityTopLeftCorner.x;
        double overlayMapOffsetY = unityCenterPoint.y - unityBotLeftCorner.y;

        // get GIS coords of overlay map
        double overlayMapXInGIS = (overlayMapOffsetX * ratioX) + GISTopLeftCorner.x;
        double overlayMapYInGIS = (overlayMapOffsetY * ratioY) + GISBotLeftCorner.y;
        string center_point = "POINT(" + overlayMapXInGIS + " " + overlayMapYInGIS + ")";
        
        // get rotation of overlay map
        float rotation = slider.value * 360;

        // get distances for BaseMap dimensions
        PostGisPoint topLeft = new PostGisPoint(GISTopLeftCorner);
        PostGisPoint topRight = new PostGisPoint(GISTopRightCorner);
        PostGisPoint botLeft = new PostGisPoint(GISBotLeftCorner);
        PostGisPoint botRight = new PostGisPoint(GISBotRightCorner);
        double baseMapWidthInKM = HaversineDistance(topLeft, topRight, DistanceUnit.Kilometers);
        double baseMapHeightInKM = HaversineDistance(topLeft, botLeft, DistanceUnit.Kilometers);

        // get pixels per KM ratio
        double horizontalPPK = baseMapWidthInKM / baseMapWidthInUnity;
        double verticalPPK = baseMapHeightInKM / baseMapHeightInUnity;

        // get pixel counts for OverlayMap
        double oMapPxlLen = 10;
        double histMapPixelWidth = oMapPxlLen * OverlayMap.transform.localScale.x;
        double histMapPixelHeight = oMapPxlLen * OverlayMap.transform.localScale.z;

        // get KM lnegths of the histrical map's sides
        double histMapWidthKM = histMapPixelWidth * horizontalPPK;
        double histMapHeightKM = histMapPixelHeight * verticalPPK;

        // get the image data into hex array
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\histMap.png";
        byte[] imgData = File.ReadAllBytes(filePath);
        string imageHex = string.Concat(imgData.Select(b => b.ToString("X2")).ToArray());

        /// TODO do the api insert call... we have the info!!!
        /// km_width = histMapWidthKM (double variable)
        /// km_height = histMapHeightKM (double variable)
        /// rotation = rotation (float variable)
        /// Center point = 'POINT(overlayMapXInGIS overlayMapYInGIS)' (both are double variables)
        /// img_data = imageHex (string variable)

        ApiPostImage(center_point, imageHex, rotation, histMapHeightKM, histMapWidthKM);
    }


    public void ApiPostImage(string center_point, string imageHex, double rotation, double histMapHeightKM, double histMapWidthKM)
    {
        JsonAddImage json = new JsonAddImage()
        {
            center_point = center_point,
            image_data = imageHex,
            image_name = "historicalMap.png",
            image_rotation = rotation.ToString(),
            image_size = imageHex.Length.ToString(),
            km_height = histMapHeightKM.ToString(),
            km_width = histMapWidthKM.ToString()
        };

        string strJson = JsonUtility.ToJson(json);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addMapRequest);
        request.ContentType = "application/json; charset=utf-8";
        request.Method = "POST";

        byte[] message = new ASCIIEncoding().GetBytes(strJson);
        Stream stream = request.GetRequestStream();

        stream.Write(message, 0, message.Length);

        try
        {
            // Retrieve the API response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // TODO: Error log that status wasnt OK.
                //Debug.Log(response.StatusCode);
            }
        }
        catch (WebException e)
        {
            // TODO: Error log message.
            //Debug.Log(e);
        }
    }
    

    /// <summary>
    /// This method loads the coord properties from Database
    /// </summary>
    private void RetrieveMapCoords()
    {
        // make API call for data
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getBoundsRequest);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        JSONObject jobject = new JSONObject(new StreamReader(response.GetResponseStream()).ReadToEnd());

        // pull out the 2 points returned from the API
        for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
        {
            if (jobject.list[0].list[0].keys[i] == "bottom_right")
            {
                GISBotRightCorner = ParsePointStr(jobject.list[0].list[0].list[i].ToString());
            }
            else if (jobject.list[0].list[0].keys[i] == "top_left")
            {
                GISTopLeftCorner = ParsePointStr(jobject.list[0].list[0].list[i].ToString());
            }
        }

        // get remaining corner points based on the first 2 (we assume selection is a square)
        GISTopRightCorner = new Vector2(GISBotRightCorner.x, GISTopLeftCorner.y);
        GISBotLeftCorner = new Vector2(GISTopLeftCorner.x, GISBotRightCorner.y);
    }


    /// <summary>
    /// This method parses a point string into a vector2
    /// example: "POINT(1.2345 6.7890)" will turn into new Vector2(x: 1.2345, y: 6.7890)
    /// </summary>
    /// <param name="pntStr">string to parse</param>
    /// <returns></returns>
    private Vector2 ParsePointStr(string pntStr)
    {
        int pFrom = pntStr.IndexOf("(") + 1;
        int pTo = pntStr.IndexOf(")");
        string[] pieces = pntStr.Substring(pFrom, pTo - pFrom).Split(' ');
        Vector2 vec = new Vector2(0, 0);
        float.TryParse(pieces[0], out vec.x);
        float.TryParse(pieces[1], out vec.y);
        return vec;
    }

    /// <summary>
    /// This public method exits the application
    /// </summary>
    public void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    /// <summary>
    /// This method loads the historical map to the OverlayMap plane
    /// </summary>
    private void PlaceOverlayMap()
    {
        // Read historical png in from disk
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\histMap.jpg";
        byte[] imgData = File.ReadAllBytes(filePath);

        // place on the OverlayMap plane game object (see code in PlaceBaseMap() for example)
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imgData);

        // scale the y value for the historical plane (as we did for the BaseMap)
        float histMapRatio = (float)tex.height / (float)tex.width;
        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.mainTexture = tex;
        OverlayMap.GetComponent<Renderer>().material = mat;
        OverlayMap.transform.localScale = new Vector3(OverlayMap.transform.localScale.x,
            OverlayMap.transform.localScale.y,
            OverlayMap.transform.localScale.z * histMapRatio);

        // FLIP - this line of code makes sure the PNG doesn't flip on the object
        OverlayMap.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
    }


    /// <summary>
    /// This method handles the loading and placing of the basemap
    /// </summary>
    private void PlaceBaseMap()
    {
        // load basemap PNG in and apply to BaseMap object
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getMapRequest);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        JSONObject jobject = new JSONObject(new StreamReader(response.GetResponseStream()).ReadToEnd());

        // find position in json object of the image data
        int position = -1;
        for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
        {
            if (jobject.list[0].list[0].keys[i] == "image_data")
            {
                position = i;
                break;
            }
        }

        // pull out the image data into a byte array
        string imgString = StripJSONObject(jobject, position);
        List<byte> bitey = new List<byte>();
        for (int i = 0; i < imgString.Length; i += 2)
        {
            char[] charArr =
            {
                imgString[i],
                imgString[i + 1]
            };
            bitey.Add(Convert.ToByte(new string(charArr), 16));
        }
        byte[] imgData = bitey.ToArray();

        // create new texture, turn it into a material, and put it on the basemap plane
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imgData);
        BaseMapRatio = (float) tex.height / (float) tex.width;
        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.mainTexture = tex;
        BaseMap.GetComponent<Renderer>().material = mat;

        // scale height of basemap plane to fit the image properly
        BaseMap.transform.localScale = new Vector3(BaseMap.transform.localScale.x, 
            BaseMap.transform.localScale.y, 
            BaseMap.transform.localScale.z * BaseMapRatio);

        // FLIP - this line of code makes sure the PNG doesn't flip on the object
        BaseMap.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
    }


    /// <summary>
    /// This method strips hex string out of jsonobject
    /// </summary>
    /// <param name="jobject"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private string StripJSONObject(JSONObject jobject, int position)
    {
        // pull the correct string out and strip off all useless chars
        string imgString = jobject.list[0].list[0].list[position].ToString();
        Regex rgx = new Regex("[^a-fA-F0-9]");
        return rgx.Replace(imgString, "");
    }


    /// <summary>
    /// Returns the distance in miles or kilometers of any two
    /// latitude / longitude points.
    /// </summary>
    /// <param name="pos1">Location 1</param>
    /// <param name="pos2">Location 2</param>
    /// <param name="unit">Miles or Kilometers</param>
    /// <returns>Distance in the requested unit</returns>
    public double HaversineDistance(PostGisPoint pos1, PostGisPoint pos2, DistanceUnit unit)
    {
        double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
        var lat = (pos2.Latitude - pos1.Latitude).ToRadians();
        var lng = (pos2.Longitude - pos1.Longitude).ToRadians();
        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                 Math.Cos(pos1.Latitude.ToRadians()) * Math.Cos(pos2.Latitude.ToRadians()) *
                 Math.Sin(lng / 2) * Math.Sin(lng / 2);
        var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        return R * h2;
    }

    /// <summary>
    /// Enum for distance types
    /// </summary>
    public enum DistanceUnit { Miles, Kilometers };
}


/// <summary>
/// This is an object that represents our JSON structure for the POST request to add an image.
/// </summary>
[System.Serializable]
public class JsonAddImage
{
    public string image_name;
    public string image_data;
    public string image_size;
    public string image_rotation;
    public string center_point;
    public string km_height;
    public string km_width;
}


/// <summary>
/// Storage class for simple point data.
/// Created by Timothy J Cowen.
/// </summary>
public class PostGisPoint
{
    private double x;
    private double y;

    /// <summary>
    /// Property for internal x value
    /// </summary>
    public double X
    {
        get => x;
        set
        {
            x = value;
            IsXSet = true;
        }
    }

    /// <summary>
    /// property for internal y value
    /// </summary>
    public double Y
    {
        get => y;
        set
        {
            y = value;
            IsYSet = true;
        }
    }

    /// <summary>
    /// These property calls just return x and y coords by more relevant names
    /// </summary>
    public double Longitude => X;
    public double Latitude => Y;

    /// <summary>
    /// Bools to determine if values are set correctly
    /// </summary>
    public bool IsXSet { get; private set; }
    public bool IsYSet { get; private set; }

    /// <summary>
    /// CONSTRUCTOR
    /// converts a vec into this object
    /// </summary>
    /// <param name="vec"></param>
    public PostGisPoint(Vector2 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
    }
}

/// <summary>
/// Convert to Radians.
/// </summary>
/// <param name="val">The value to convert to radians</param>
/// <returns>The value in radians</returns>
public static class NumericExtensions
{
    public static double ToRadians(this double val)
    {
        return (Math.PI / 180) * val;
    }
}
