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
            int position = -1;

            // Loops to finds the image position and then breaks when it find it
            for (int i = 0; i < jobject.list[0].list[0].keys.Count; i++)
            {
                if (jobject.list[0].list[0].keys[i] == "image_data")
                {
                    position = i;
                    break;
                }
            }
            
            // Take the Hex string found in the JSON and then remove all none Hex 
            // characters that might of sneaked in
            string imgString = jobject.list[0].list[0].list[position].ToString();
            imgString = imgString.StripToHex();
           
            
            // Here we seperate the string into hex pair
            // which then converts into a byte and placed into
            // an array
            List<byte> bitey = new List<byte>();
            for (int i = 0; i < imgString.Length; i++)
            {
                char[] charArr =
                {
                    imgString[i],
                    imgString[++i]
                };
                string biteme = new string(charArr);
                byte number = Convert.ToByte(biteme, 16);
                bitey.Add(number);
            }
            imgData = bitey.ToArray();
 
        }
        else
        {
            imgData = null;
        }

        return imgData;
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