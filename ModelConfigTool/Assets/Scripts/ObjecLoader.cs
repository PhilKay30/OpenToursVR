/// File: ObjecLoader.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: March 20th, 2020
/// Description: This file contains a class for loading and savings a model

using UnityEngine;
using Dummiesman;
using System.IO;
using System.IO.Compression;
using System;
using System.Text;
using System.Linq;

public class ObjecLoader : MonoBehaviour
{
    /// <summary>
    /// this object contains the orientation script to allow it's Model to be set (so it can control the object)
    /// </summary>
    public GameObject ScriptHandle;

    /// <summary>
    /// Public shader to include it in compilation
    /// </summary>
    public Shader shader;

    /// <summary>
    /// The main camera
    /// </summary>
    public Camera MainCamera;

    /// <summary>
    /// This is is the Model that is loaded from file
    /// </summary>
    private GameObject Model;

    /// <summary>
    /// This is the plane to show the osm map on
    /// </summary>
    public GameObject MapPlane;
    
    /// <summary>
    /// Create api handler
    /// </summary>
    private API_Handler api = new API_Handler();

    /// <summary>
    /// Map Bounds for OSM map
    /// </summary>
    private Vector2 TopLeft = new Vector2();
    private Vector2 BotRight = new Vector2();

    /// <summary>
    /// a Point representing the GIS coord of the Model
    /// </summary>
    float pntLongitude;
    float pntLatitude;

    /// <summary>
    /// The full path of the model file
    /// </summary>
    private string FilePath;

    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        try
        {
            GetMapBounds();
            LoadOSMMap();
            LoadModel();
        }
        catch (Exception e)
        {
            using (StreamWriter sw = File.CreateText(@"C:\Users\p_kem\Desktop\log.txt"))
            {
                sw.WriteLine(e.ToString());
            }
        }
    }

    /// <summary>
    /// Sets the map bounds from an api call
    /// </summary>
    private void GetMapBounds()
    {
        var mapBounds = api.GetMapBounds();
        BotRight = new Vector2((float)mapBounds[0]["longitude"], (float)mapBounds[0]["latitude"]);
        TopLeft = new Vector2((float)mapBounds[1]["longitude"], (float)mapBounds[1]["latitude"]);
    }

    /// <summary>
    /// Loads OSM map from api and displays it on plane
    /// </summary>
    private void LoadOSMMap()
    {
        // Get the osm data and create a texture out of it
        var osmMapData = api.GetOsmMap();
        Texture2D mapTexture = new Texture2D(2, 2);
        mapTexture.LoadImage(osmMapData);

        // Scale the plane appropriately
        float scaleX = mapTexture.width / 400f;
        float scaleZ = mapTexture.height / 400f;
        MapPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);

        // Wrap the texture in a Material and apply it to the plane
        Material material = new Material(shader);
        material.renderQueue = 2998; //to fix the clipping issue
        material.mainTexture = mapTexture;
        MapPlane.GetComponent<Renderer>().material = material;

        // Fix the upsidedown PNG...
        MapPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, -1));
    }

    /// <summary>
    /// Load the model onto the plane
    /// </summary>
    private void LoadModel()
    {
        // read the meta file to find out where we get the model from (and its' GIS coords)
        string[] lines = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR\\Models\\ModelData.txt");
        FilePath = lines[0];
        pntLongitude = float.Parse(lines[1]);
        pntLatitude = float.Parse(lines[2]);

        // Make sure file exists
        if (!File.Exists(FilePath))
        {
            Debug.Log("File Doesn't Exist");
        }
        else
        {
            // file exists, delete any previous model info (fail-safe)
            if (Model != null)
            {
                Destroy(Model);
            }

            // Get the game object from the location on disk (also scale the model down to 10% size...)
            Model = new OBJLoader().Load(FilePath);
            Model.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Assign the model to the Model Control script so user can control it
            MainCamera.GetComponent<CamController>().Model = Model;
            MapPlane.GetComponent<ModelController>().Model = Model;

            // move the model to where it's supposed to be on plane
            Model.transform.position = GisToUnity(new Vector2(pntLongitude, pntLatitude));
            Vector3 currModel = Model.transform.position;
            MainCamera.transform.position = new Vector3(currModel.x, currModel.y + 30, currModel.z - 75);
            MainCamera.transform.LookAt(currModel);
        }
    }


    /// <summary>
    /// BUTTON CLICK HANDLER
    /// Saves the model / data to db
    /// </summary>
    public void SaveModel()
    {
        DeleteTempFiles();
        CopyModelFiles();
        ZipFiles();
        SaveToApi();
        DeleteTempFiles();
    }

    /// <summary>
    /// This model objects saves the current model to the DB
    /// </summary>
    private void SaveToApi()
    {
        // here we get all the info to save
        ModelObj model = new ModelObj();
        byte[] imgData = File.ReadAllBytes(@"MyFolder\Temp\myZip.zip");
        model.model_data = string.Concat(imgData.Select(b => b.ToString("X2")).ToArray());
        model.model_offset = Model.transform.position.y;
        model.model_location = string.Format("POINT({0} {1})", pntLongitude, pntLatitude);
        model.model_rotation = JsonUtility.ToJson(Model.transform.rotation);
        model.model_scaling = JsonUtility.ToJson(Model.transform.localScale);
        // do the save
        api.AddModel(model);
    }

    /// <summary>
    /// This method zips the current model into a temp directory
    /// </summary>
    private void ZipFiles()
    {
        CompressDirectory("MyFolder", @"MyFolder\Temp\myZip.zip");
    }

    /// <summary>
    /// This method unzips a zip files
    /// </summary>
    private void UnzipFiles()
    {
        DecompressToDirectory(@"MyFolder\Temp\myZip.zip", @"C:\Users\p_kem\Desktop\Folder");
    }

    /// <summary>
    /// This method clears the MyFolder out and creates the Temp directory anew
    /// </summary>
    private void DeleteTempFiles()
    {
        EmptyFolder("MyFolder");
        Directory.CreateDirectory("MyFolder\\Temp");
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
    /// This method uplls the current model files out of source directory and puts them into
    /// MyFolder directory
    /// </summary>
    private void CopyModelFiles()
    {
        // Split the filepath into its' pieces
        string[] filePathPcs = FilePath.Split('\\');
        string[] namePcs = filePathPcs[filePathPcs.Length - 1].Split('.');
        string fileName = namePcs[0];

        // pull out the root path the obj is located in
        string basePath = "";
        for (int i = 0; i < filePathPcs.Length - 1; i++)
        {
            basePath += filePathPcs[i] + "\\";
        }

        // make sure the MyFolder directory exists
        if (!Directory.Exists("MyFolder"))
        {
            Directory.CreateDirectory("MyFolder");
        }

        // copy the obj and material files over
        File.Copy(FilePath, string.Format("MyFolder\\{0}", filePathPcs[filePathPcs.Length - 1]));
        string[] pcs = FilePath.Split('.');
        string mtlFile = pcs[0] + ".mtl";
        File.Copy(mtlFile, string.Format("MyFolder\\{0}", fileName + ".mtl"));
        string subFolder = basePath + fileName;

        // check if any image files go with this obj model
        if (Directory.Exists(subFolder))
        {
            // found a folder we need to copy, duplicate the folder and copy everything in
            Directory.CreateDirectory("MyFolder\\" + fileName);
            string[] filesToCopy = Directory.GetFiles(subFolder);
            foreach (string fPath in filesToCopy)
            {
                string[] buff = fPath.Split('\\');
                File.Copy(fPath, "MyFolder\\" + fileName + "\\" + buff[buff.Length - 1]);
            }
        }
    }


    /// <summary>
    /// Converts a GIS coord into a Unity coord
    /// </summary>
    /// <param name="gisCoords">gis coord to convert</param>
    /// <returns>Unity coord</returns>
    private Vector3 GisToUnity(Vector2 gisCoords)
    {
        // get the width of map and width it is in GIS and get ratios to apply to 
        double mapPixelWidth = 10f * (double)MapPlane.transform.localScale.x;
        double mapPixelLength = 10f * (double)MapPlane.transform.localScale.z;
        double mapGISWidth = BotRight.x - TopLeft.x;
        double mapGISLength = TopLeft.y - BotRight.y;
        var pixelsToGISRatioWidth = mapPixelWidth / mapGISWidth;
        var pixelsToGISRatioLength = mapPixelLength / mapGISLength;

        // find bottom left corner of plane
        var botLeftMapLocation = new Vector3();
        botLeftMapLocation.x = (float)(MapPlane.transform.position.x - (mapPixelWidth / 2));
        botLeftMapLocation.y = MapPlane.transform.position.y;
        botLeftMapLocation.z = (float)(MapPlane.transform.position.z - (mapPixelLength / 2));

        // Find distance we need to move in GIS values
        double distanceToMovePointLogitude = gisCoords.x - TopLeft.x;
        double distanceToMovePointLatitude = gisCoords.y - BotRight.y;

        // Apply the ratios to the GIS dimensions to get pixel count distance for Unity
        double distanceToMoveInUnityX = distanceToMovePointLogitude * pixelsToGISRatioWidth;
        double distanceToMoveInUnityZ = distanceToMovePointLatitude * pixelsToGISRatioLength;

        // x and z coords (in unity) for the data point
        float dataPointX = (float)(distanceToMoveInUnityX + botLeftMapLocation.x);
        float dataPointZ = (float)(distanceToMoveInUnityZ + botLeftMapLocation.z);

        // This is the position the datapoint needs to be placed at (in unity coord system)
        // NOTE: The y value is a public property you can fiddel with in the IDE at runtime
        return new Vector3(dataPointX, 0, dataPointZ);
    }

    /// <summary>
    /// This method zips a file
    /// </summary>
    /// <param name="sDir">source dir</param>
    /// <param name="sRelativePath">relative path to src dir</param>
    /// <param name="zipStream">zip stream to use for zipping</param>
    private void CompressFile(string sDir, string sRelativePath, GZipStream zipStream)
    {
        //Compress file name
        char[] chars = sRelativePath.ToCharArray();
        zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
        foreach (char c in chars)
            zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

        //Compress file content
        byte[] bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
        zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
        zipStream.Write(bytes, 0, bytes.Length);
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
    /// This method zips a directory
    /// </summary>
    /// <param name="sInDir">directory to sip</param>
    /// <param name="sOutFile">path/name of zip file to create</param>
    private void CompressDirectory(string sInDir, string sOutFile)
    {
        string[] sFiles = Directory.GetFiles(sInDir, "*.*", SearchOption.AllDirectories);
        int iDirLen = sInDir[sInDir.Length - 1] == Path.DirectorySeparatorChar ? sInDir.Length : sInDir.Length + 1;

        using (FileStream outFile = new FileStream(sOutFile, FileMode.Create, FileAccess.Write, FileShare.None))
        using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
            foreach (string sFilePath in sFiles)
            {
                string sRelativePath = sFilePath.Substring(iDirLen);
                CompressFile(sInDir, sRelativePath, str);
            }
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
            while (DecompressFile(sDir, zipStream)) ;
    }
}
