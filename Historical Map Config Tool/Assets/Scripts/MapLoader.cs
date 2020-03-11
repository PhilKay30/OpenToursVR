using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public class MapLoader : MonoBehaviour
{
    public GameObject BaseMap;
    public GameObject OverlayMap;

    private string apiRequest = "http://192.0.203.84:5000/getimg/osmMap.png";

    private Vector2 GISTopLeftCorner { get; set; }
    private Vector2 GISTopRightCorner { get; set; }
    private Vector2 GISBotLeftCorner { get; set; }
    private Vector2 GISBotRightCorner { get; set; }

    private float BaseMapRatio { get; set; }


    /// <summary>
    /// This method loads the 2 map files into into the application
    /// </summary>
    void Start()
    {
        RetrieveMapCoords();
        PlaceBaseMap();
        PlaceOverlayMap();
    }


    /// <summary>
    /// This method will be called from a button click to save the historical map / historical map data to the DB
    /// </summary>
    public void SaveMap()
    {
        // Unity coords for BaseMap
        Vector2 basemapPosition = new Vector2(BaseMap.transform.position.x, BaseMap.transform.position.z);

        Vector2 unityTopLeftCorner = new Vector2(basemapPosition.x - 5, basemapPosition.y + (5 * BaseMapRatio));
        Vector2 unityTopRightCorner = new Vector2(basemapPosition.x + 5, basemapPosition.y + (5 * BaseMapRatio));
        Vector2 unityBotLeftCorner = new Vector2(basemapPosition.x - 5, basemapPosition.y - (5 * BaseMapRatio));
        Vector2 unityBotRightCorner = new Vector2(basemapPosition.x + 5, basemapPosition.y - (5 * BaseMapRatio));

        // Unity coord for center of OverlayMap
        Vector2 unityCenterPoint = new Vector2(OverlayMap.transform.position.x, OverlayMap.transform.position.z);

        // Maths!
        double baseMapWidthInGIS = GISTopRightCorner.x - GISTopLeftCorner.x;
        double baseMapWidthInUnity = unityTopRightCorner.x - unityTopLeftCorner.x;
        double baseMapHeightInGIS = GISTopLeftCorner.y - GISBotLeftCorner.y;
        double baseMapHeightInUnity = unityTopLeftCorner.y - unityBotLeftCorner.y;

        double ratioX = baseMapWidthInGIS / baseMapWidthInUnity;
        double ratioY = baseMapHeightInGIS / baseMapHeightInUnity;

        double overlayMapOffsetX = unityCenterPoint.x - unityTopLeftCorner.x;
        double overlayMapOffsetY = unityCenterPoint.y - unityBotLeftCorner.y;

        double overlayMapXInGIS = (overlayMapOffsetX * ratioX) + GISTopLeftCorner.x;
        double overlayMapYInGIS = (overlayMapOffsetY * ratioY) + GISBotLeftCorner.y;

    }



    /// <summary>
    /// This method loads the coord properties from Database
    /// </summary>
    private void RetrieveMapCoords()
    {
        // TODO: get coords from API once the API has this functionality
    }


    /// <summary>
    /// This method loads the historical map to the OverlayMap plane
    /// </summary>
    private void PlaceOverlayMap()
    {
        // TODO
        // Read historical png in from disk
        // place on the OverlayMap plane game object (see code in PlaceBaseMap() for example)
        // scale the y value for the historical plane (as we did for the BaseMap)
        // FLIP the materials texture so it's not upside down!
    }


    /// <summary>
    /// This method handles the loading and placing of the basemap
    /// </summary>
    private void PlaceBaseMap()
    {
        // load basemap PNG in and apply to BaseMap object
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiRequest);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        JSONObject jobject = new JSONObject(new StreamReader(response.GetResponseStream()).ReadToEnd());

        int position = -1;
        for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
        {
            if (jobject.list[0].list[0].keys[i] == "image_data")
            {
                position = i;
                break;
            }
        }

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

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imgData);
        BaseMapRatio = (float) tex.height / (float) tex.width;
        Material mat = new Material(Shader.Find("Diffuse"));
        mat.mainTexture = tex;
        BaseMap.GetComponent<Renderer>().material = mat;

        // still need to scale height of basemap object
        BaseMap.transform.localScale = new Vector3(BaseMap.transform.localScale.x, 
            BaseMap.transform.localScale.y, 
            BaseMap.transform.localScale.z * BaseMapRatio);

        // FLIP - this line of code makes sure the PNG doesn't flip on the object
        BaseMap.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
    }


    private string StripJSONObject(JSONObject jobject, int position)
    {
        string imgString = jobject.list[0].list[0].list[position].ToString();
        Regex rgx = new Regex("[^a-fA-F0-9]");
        return rgx.Replace(imgString, "");
    }
}
