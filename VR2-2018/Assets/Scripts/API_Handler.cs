using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Networking;

public class API_Handler
{

    //private string apiRequest = "http://10.192.114.53:5000/getimg/osmMap.png";
    private string osmMapApiRequest = "http://192.0.203.84:5000/getimg/osmMap.png";
    private string dataPointsApiRequest = "http://192.0.203.84:5000/getpoint/";
    private string mapBoundsApiRequest = "http://192.0.203.84:5000/getbounds/osmMap";
    private string dataPointInformation = "http://192.0.203.84:5000/getpoint/";
    private string histMapApiRequest = "http://192.0.203.84:5000/getimg/historicalMap.png";

    /// <summary>
    /// Vector to hold the km dimensions of the osm map
    /// </summary>
    public Vector2 OsmMapDimensions = new Vector2();


    /// <summary>
    /// This method gets the historical map from database and return the obj
    /// </summary>
    /// <returns></returns>
    public HistMapObj GetHistMap()
    {
        JSONObject jobject = MakeWebRequest(histMapApiRequest);
        HistMapObj obj = new HistMapObj();

        if (jobject != null)
        {
            for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
            {
                switch (jobject.list[0].list[0].keys[i])
                {
                    case "image_data":
                        obj.MapData = HexStringToBinary(jobject.list[0].list[0].list[i].ToString());
                        break;
                    case "center_point":
                        obj.CenterPoint = ParsePointStr(jobject.list[0].list[0].list[i].ToString());
                        break;
                    case "image_rotation":
                        obj.Rotation = StringToFloat(jobject.list[0].list[0].list[i].ToString());
                        break;
                    case "km_height":
                        obj.HeightKM = StringToFloat(jobject.list[0].list[0].list[i].ToString());
                        break;
                    case "km_width":
                        obj.WidthKM = StringToFloat(jobject.list[0].list[0].list[i].ToString());
                        break;
                    default:
                        // this is data we don't need here
                        break;
                }
            }
        }
        return obj;
    }

    /// <summary>
    /// This method converts a string into a double
    /// </summary>
    /// <param name="inputStr">string to get double out of</param>
    /// <returns>parsed double</returns>
    public float StringToFloat(string inputStr)
    {
        float outputVal = -1;
        if (!float.TryParse(inputStr, out outputVal))
        {
            // string didn't contain a number
            // TODO: Exception possibility left in for testing, should be caught upon release build
        }
        return outputVal;
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
    /// This method will make an Web request to the API to retrieved 
    /// JSON which will then be parsed to retrieved the image Hex 
    /// infomartion which will then be converted into a byte array
    /// </summary>
    /// <returns></returns>
    public byte[] GetOsmMap()
    {
        JSONObject jobject = new JSONObject();
        byte[] imgData = null;
        jobject = MakeWebRequest(osmMapApiRequest);


        // If the request didn't return null I will find 
        // where the image data position is and then convert it to byte array
        if (jobject != null)
        {
            // Loops to finds the image position and then breaks when it find it
            for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
            {
                if (jobject.list[0].list[0].keys[i] == "image_data")
                {
                    imgData = HexStringToBinary(jobject.list[0].list[0].list[i].ToString());
                    
                }
                else if (jobject.list[0].list[0].keys[i] == "km_height")
                {
                    OsmMapDimensions.y = StringToFloat(jobject.list[0].list[0].list[i].ToString());
                }
                else if (jobject.list[0].list[0].keys[i] == "km_width")
                {
                    OsmMapDimensions.x = StringToFloat(jobject.list[0].list[0].list[i].ToString());
                }
            }
        }
        else
        {
            imgData = null;
        }

        return imgData;
    }


    /// <summary>
    /// Converts a hex encoding string into binary data
    /// </summary>
    /// <param name="hexStr">string to convert</param>
    /// <returns>the binary array</returns>
    private byte[] HexStringToBinary(string hexStr)
    {
        string strBuff = hexStr.StripToHex();
        List<byte> bitey = new List<byte>();
        for (int i = 0; i < strBuff.Length; i++)
        {
            char[] charArr =
            {
                    strBuff[i],
                    strBuff[++i]
            };
            string biteme = new string(charArr);
            byte number = Convert.ToByte(biteme, 16);
            bitey.Add(number);
        }
        return bitey.ToArray();
    }


    /// <summary>
    /// This will connect to the API and make the web request 
    /// and return it in JSON format
    /// </summary>
    /// <param name="apiCall">The string of the web address to make the call</param>
    /// <returns>JSONObject of the response</returns>
    private JSONObject MakeWebRequest(string apiCall)
    {
        HttpWebResponse response;
        HttpWebRequest request;
        JSONObject jobject = new JSONObject();
        try
        {
            request = (HttpWebRequest)WebRequest.Create(apiCall);
            response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            jobject = new JSONObject(jsonResponse);
            Debug.Log(jobject);
        }
        catch (Exception e)
        {
            Debug.Log("An error connecting to the API happened: " + e.Message);
        }
        return jobject;
    }





    /// <summary>
    /// This will make the API call to get the point ID
    /// the Longitude and Latitude points 
    /// </summary>
    /// <returns></returns>
    public List<Dictionary<string, double>> GetPointLocations()
    {
        List<Dictionary<string, double>> keyValuePairs = new List<Dictionary<string, double>>(); //that's just what VS recommended I call this variable

        JSONObject datapoints = new JSONObject();

        datapoints = MakeWebRequest(dataPointsApiRequest);

        foreach (var lists in datapoints.list[0].list)
        {
            Dictionary<string, double> values = new Dictionary<string, double>();

            for (int i = 0; i < lists.keys.Count; i++)
            {
                
                if (lists.keys[i] == "point_id")
                {
                    values["id"] = Convert.ToDouble(lists.list[i].ToString());
                }
                if (lists.keys[i] == "point_location")
                {
                    SplitPoints(lists.list[i].ToString(), ref values);
                }
            }
            keyValuePairs.Add(values);
        }
        

        return keyValuePairs;
    }




    private void SplitPoints(string point, ref Dictionary<string, double> values)
    {
        int from = point.IndexOf("(") + "(".Length;
        int to = point.LastIndexOf(")");
        point = point.Substring(from, to - from);
        string[] points = point.Split(' ');

        // Here there be magic jazz hands
        values["longitude"] = Convert.ToDouble(points[0]);
        values["latitude"] = Convert.ToDouble(points[1]);
    }




    public Dictionary<string, string> GetPointInformation(double _id)
    {
        string id = _id.ToString();
        string api = dataPointInformation + id;

        JSONObject dpInfo = new JSONObject();
        dpInfo = MakeWebRequest(api);
        dpInfo = dpInfo.list[0].list[0];
        Dictionary<string, string> info = new Dictionary<string, string>();
        
        for(int i = 0; i < dpInfo.Count; i++)
        {
            
            info[dpInfo.keys[i].ToString()] = dpInfo.list[i].ToString();
        }


        return info;        
    }



    public List<Dictionary<string, double>> GetMapBounds()
    {
        List<Dictionary<string, double>> keyValuePairs = new List<Dictionary<string, double>>();

        JSONObject bounds = new JSONObject();
        bounds = MakeWebRequest(mapBoundsApiRequest);
        bounds = bounds.list[0].list[0];

        for(int i = 0; i < bounds.list.Count; i++)
        {
            Dictionary<string, double> values = new Dictionary<string, double>();
            if (bounds.keys[i] == "top_left")
            {
                values["top_left"] = i;
                SplitPoints(bounds.list[i].ToString(), ref values);
            }
            if (bounds.keys[i] == "bottom_right")
            {
                values["bottom_right"] = i;
                SplitPoints(bounds.list[i].ToString(), ref values);
            }

            if (values.Count != 0)
            {
                keyValuePairs.Add(values);
            }
            
            
        }

        return keyValuePairs;
    }
}


public static class StringExtensions
{
    public static string StripToHex(this string inputString)
    {
        Regex rgx = new Regex("[^a-fA-F0-9]");
        return rgx.Replace(inputString, "");
    }
}


/// <summary>
/// This class represents the historical map
/// </summary>
public class HistMapObj
{
    public byte[] MapData { get; set; }
    public float Rotation { get; set; }
    public float WidthKM { get; set; }
    public float HeightKM { get; set; }
    public Vector2 CenterPoint { get; set; }

    /// <summary>
    /// CONSTURCTOR
    /// Assigns all properties initial (invalid) values
    /// </summary>
    public HistMapObj()
    {
        MapData = new byte[1];
        Rotation = -1;
        WidthKM = -1;
        HeightKM = -1;
        CenterPoint = new Vector2();
    }
}