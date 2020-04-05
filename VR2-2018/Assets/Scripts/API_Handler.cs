using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Dummiesman;
using System.Linq;

using UnityEngine;
using UnityEngine.Networking;

public class API_Handler
{

    private string osmMapApiRequest = "http://192.0.203.84:5000/getimg/osmMap.png";
    private string dataPointsApiRequest = "http://192.0.203.84:5000/getpoint/";
    private string mapBoundsApiRequest = "http://192.0.203.84:5000/getbounds/osmMap";
    private string dataPointInformation = "http://192.0.203.84:5000/getpoint/";
    private string histMapApiRequest = "http://192.0.203.84:5000/getimg/historicalMap.png";
    private string getModelsApiRequest = "http://192.0.203.84:5000/getmodel/";

    /// <summary>
    /// Vector to hold the km dimensions of the osm map
    /// </summary>
    public Vector2 OsmMapDimensions = new Vector2();


    /// <summary>
    /// This method gets all missing models from database, instantiates all models, and returns thier orientation data
    /// </summary>
    /// <returns></returns>
    public List<ModelHandle> GetModels()
    {
        // Get and parse request into model fragments (thing still only have id, roation, and location)
        JSONObject jobject = MakeWebRequest(getModelsApiRequest);
        List<ModelFrag> frags = new List<ModelFrag>();

        // loop through each models returned
        for (int i = 0; i < jobject.list[0].list.Count; i++)
        {
            JSONObject jo = jobject.list[0].list[i];
            ModelFrag frag = new ModelFrag();

            // loop through each field of the model, saving everything into a fragment object (in a list)
            for (int j = 0; j < jo.list.Count; j++)
            {
                switch (j)
                {
                    case 0:
                        frag.model_id = Int32.Parse(jo[j].ToString());
                        break;
                    case 1:
                        frag.model_location = jo[j].ToString();
                        break;
                    case 2:
                        frag.model_rotation = jo[j].ToString();
                        break;
                    default:
                        break;
                }
            }
            frags.Add(frag);
        }

        // create model directory if it doesn't exist
        if (!Directory.Exists("Models"))
        {
            Directory.CreateDirectory("Models");
        }

        // Get all missing model data
        foreach (ModelFrag frag in frags)
        {
            if (!Directory.Exists("Models\\" + frag.model_id))
            {
                // found a model that we don't already have
                // make web request to get the rest of the data
                string[] lines = new string[4];
                Directory.CreateDirectory("Models\\" + frag.model_id);
                JSONObject jsonData = MakeWebRequest(getModelsApiRequest + frag.model_id);
                jsonData = jsonData.list[0].list[0];

                // loop through the data returned for the model
                for (int i = 0; i < jsonData.keys.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            // we found the model zip data
                            // write zip file to disk
                            List<byte> bitey = new List<byte>();
                            string imgString = jsonData[i].ToString();
                            imgString = imgString.StripToHex();
                            for (int j = 0; j < imgString.Length; j += 2)
                            {
                                char[] charArr =
                                {
                                    imgString[j],
                                    imgString[j + 1]
                                };
                                bitey.Add(Convert.ToByte(new string(charArr), 16));
                            }
                            if (File.Exists("Models\\temp.zip"))
                            {
                                // delete any previous zip folder
                                File.Delete("Models\\temp.zip");
                            }
                            File.WriteAllBytes("Models\\temp.zip", bitey.ToArray());

                            // unzip to the Models\\frag.model_id folder
                            DecompressToDirectory("Models\\temp.zip", "Models\\" + frag.model_id);

                            // create meta file
                            lines[0] = frag.model_rotation;
                            lines[1] = frag.model_location;
                            break;
                        case 1:
                            // it's the offset
                            lines[2] = jsonData[i].ToString();
                            break;
                        case 2:
                            // it's the scaling quat
                            lines[3] = jsonData[i].ToString();
                            break;
                        default:
                            break;
                    }
                }
                File.WriteAllLines("Models\\" + frag.model_id + "\\meta.txt", lines);
            }
        } // end of getting missing model data

        // Get the Folder path IDs and the frag ids, and use a third list to perform an except operation on list1 with list2 to find the differences in them, delete the differences.
        string[] folders = Directory.GetDirectories("Models");
        List<string> fragIDS = new List<string>();
        foreach (ModelFrag frag in frags)
        {
            fragIDS.Add(frag.model_id);
        }
        List<string> folderID = new List<string>();
        foreach (string folder in folders)
        {
            folderID.Add(folder.Split("\\")[1]);
        }
        List<string> PathToDelete = folderID.Except(fragIDS).ToList();
        foreach (string path in PathToDelete)
        {
            EmptyFolder(path);
        }
       
				
				
        // time to load all models
        // get all models into a list to return
        List<ModelHandle> models = new List<ModelHandle>();
        string[] folders = Directory.GetDirectories("Models");

        // loop through all files in each directory to find the obj and metadata files
        foreach (string fold in folders)
        {
            ModelHandle newModel = new ModelHandle();
            string[] files = Directory.GetFiles(fold);
            foreach (string fileStr in files)
            {
                if (fileStr.EndsWith(".obj"))
                {
                    // found the obj file, make the object
                    newModel.GameObj = new OBJLoader().Load(fileStr);
                }
                if (fileStr.EndsWith("meta.txt"))
                {
                    // found the metadata file
                    // parse it and save to object
                    string[] metaLines = File.ReadAllLines(fileStr);
                    newModel.Rotation = JsonUtility.FromJson<Quaternion>(Destringify(metaLines[0]));
                    newModel.Position = ParsePointStr(Destringify(metaLines[1]));
                    newModel.Offset = float.Parse(Destringify(metaLines[2]));
                    newModel.Scale = JsonUtility.FromJson<Vector3>(Destringify(metaLines[3]));
                }
            }
            models.Add(newModel);
        }

        // return the list of all model handles (they still need to have their orientation data applied to them)
        return models;
    }



    /// <summary>
    /// This method clears out a folder completely
    /// </summary>
    /// <param name="baseFolder">folder to clear out</param>
    private void EmptyFolder(string baseFolder)
    {
        if (Directory.Exists(baseFolder))
        {
            // delete all files
            string[] filePaths = Directory.GetFiles(baseFolder);
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }

            // recursively delete any subfolder contents
            string[] folderPaths = Directory.GetDirectories(baseFolder);
            foreach (string folder in folderPaths)
            {
                EmptyFolder(folder);
                Directory.Delete(folder);
            }

        }
    }


    /// <summary>
    /// This method removes all garbage characters from a printed JSON string to make it deserializeable
    /// </summary>
    /// <param name="sinput"></param>
    /// <returns></returns>
    private string Destringify(string sinput)
    {
        string s = sinput.Replace("\\", "");
        s = s.TrimStart('"');
        s = s.TrimEnd('"');
        return s;
    }


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


    /// <summary>
    /// This method splits a point string
    /// </summary>
    /// <param name="point">point string to split</param>
    /// <param name="values">dict of split up values</param>
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
    /// This method gets point info from db
    /// </summary>
    /// <param name="_id">id of point to get data for</param>
    /// <returns></returns>
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


    /// <summary>
    /// This method gets the map boundaries
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// This method unzips a file
    /// </summary>
    /// <param name="sDir">src directory</param>
    /// <param name="zipStream">zip stream to unzip with</param>
    /// <returns></returns>
    private bool DecompressFile(string sDir, GZipStream zipStream)
    {
        //Decompress file name
        byte[] bytes = new byte[sizeof(int)];
        int Readed = zipStream.Read(bytes, 0, sizeof(int));
        if (Readed < sizeof(int))
            return false;

        int iNameLen = BitConverter.ToInt32(bytes, 0);
        bytes = new byte[sizeof(char)];
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < iNameLen; i++)
        {
            zipStream.Read(bytes, 0, sizeof(char));
            char c = BitConverter.ToChar(bytes, 0);
            sb.Append(c);
        }
        string sFileName = sb.ToString();

        //Decompress file content
        bytes = new byte[sizeof(int)];
        zipStream.Read(bytes, 0, sizeof(int));
        int iFileLen = BitConverter.ToInt32(bytes, 0);

        bytes = new byte[iFileLen];
        zipStream.Read(bytes, 0, bytes.Length);

        string sFilePath = Path.Combine(sDir, sFileName);
        string sFinalDir = Path.GetDirectoryName(sFilePath);
        if (!Directory.Exists(sFinalDir))
            Directory.CreateDirectory(sFinalDir);

        using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            outFile.Write(bytes, 0, iFileLen);

        return true;
    }

    /// <summary>
    /// This method decompresses a zipped folder to a directory
    /// </summary>
    /// <param name="sCompressedFile">zip file to decompress</param>
    /// <param name="sDir">output directory for contents</param>
    private void DecompressToDirectory(string sCompressedFile, string sDir)
    {
        using (FileStream inFile = new FileStream(sCompressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
        using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
            while (DecompressFile(sDir, zipStream));
    }
}


/// <summary>
/// This class contains string extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// This method strips all bad chars out of a hex string
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

/// <summary>
/// This class represents a Model
/// </summary>
public class ModelHandle
{
    public GameObject GameObj { get; set; } // GameObject handle for the model
    public Quaternion Rotation { get; set; } // the rotation to apply
    public Vector3 Scale { get; set; } // the scale to apply
    public Vector2 Position { get; set; } // the position to apply
    public float Offset { get; set; } // the y offset to apply
}

/// <summary>
/// This class represents a piece of model data
/// </summary>
public class ModelFrag
{
    public int model_id; // db id
    public string model_rotation; // rotation of model
    public string model_location; // GIS point of model
}