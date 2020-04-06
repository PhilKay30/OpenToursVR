/// File: API_Handler.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file contains button handlers for the UI

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

public class API_Handler
{

    //private string apiRequest = "http://10.192.114.53:5000/getimg/osmMap.png";
    private string osmMapApiRequest = "http://192.0.203.84:5000/getimg/osmMap.png";
    private string mapBoundsApiRequest = "http://192.0.203.84:5000/getbounds/osmMap";
    private string addModelApiRequest = "http://192.0.203.84:5000/addmodel/";

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
                    break;
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
    public byte[] HexStringToBinary(string hexStr)
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
        }
        catch (Exception e)
        {
            Debug.Log("An error connecting to the API happened: " + e.Message);
        }
        return jobject;
    }


    /// <summary>
    /// This method adds a model to the DB
    /// </summary>
    /// <param name="model"></param>
    public void AddModel(ModelObj model)
    {
        string strJson = JsonUtility.ToJson(model);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addModelApiRequest);
        request.ContentType = "application/json; charset=utf-8";
        request.Method = "POST";
        byte[] message = new ASCIIEncoding().GetBytes(strJson);
        Stream stream = request.GetRequestStream();
        stream.Write(message, 0, message.Length);
        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Debug.Log(response.StatusCode);
            }
        }
        catch (Exception e)
        {
            Debug.Log("API_Handler Exception: " + e.ToString());
        }
    }



    /// <summary>
    /// This method splits a POINT string into its pieces
    /// </summary>
    /// <param name="point">string to split</param>
    /// <param name="values">OUT param to hold values</param>
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


    /// <summary>
    /// This methods gets map boundaries from db
    /// </summary>
    /// <returns></returns>
    public List<Dictionary<string, double>> GetMapBounds()
    {
        List<Dictionary<string, double>> keyValuePairs = new List<Dictionary<string, double>>();

        JSONObject bounds = new JSONObject();
        bounds = MakeWebRequest(mapBoundsApiRequest);
        bounds = bounds.list[0].list[0];

        for (int i = 0; i < bounds.list.Count; i++)
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


/// <summary>
/// This class represents a set of string extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// This extension method strips everything except for hex chars out of a string
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string StripToHex(this string inputString)
    {
        Regex rgx = new Regex("[^a-fA-F0-9]");
        return rgx.Replace(inputString, "");
    }
}


/// <summary>
/// This object represents a Model
/// </summary>
[System.Serializable]
public class ModelObj
{
    public string model_location;
    public string model_rotation;
    public string model_scaling;
    public string model_data;
    public float model_offset;
}