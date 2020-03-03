using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

public class API_Handler
{
   
    private string apiRequest = "http://192.0.203.84:5000/db_api/getimg/osmMap.png";
    public byte[] GetOsmMap()
    {
        //SendWebRequest req;
        HttpWebResponse response;
        HttpWebRequest request;
        JSONObject jobject = new JSONObject();
        byte[] imgData = null;
        try
        {
            request = (HttpWebRequest)WebRequest.Create(apiRequest);
            response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            jobject = new JSONObject(jsonResponse);

        }
        catch (Exception e)
        {
            Debug.Log("An error connecting to the API happened: " + e.Message);
        }

        if (jobject != null)
        {
            int position = -1;
            for (int i = 0; i < jobject.list[0].keys.Count; i++)
            {
                if (jobject.list[0].keys[i] == "img")
                {
                    position = i;
                    break;
                }
            }
            string imgString = jobject.list[0].list[position].ToString().Replace("{", "");
            imgString = imgString.Replace(",", "");
            imgString = imgString.Replace("}", "");
            imgString = imgString.Replace("\"", "");
           
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

    
}
