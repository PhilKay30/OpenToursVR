using System;
using System.Collections;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Threading;
using System.Threading.Tasks;
=======
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
using UnityEngine;

public class API_Data_Loader : MonoBehaviour
{
<<<<<<< HEAD
    [SerializeField]
    private GameObject tp;
    [SerializeField]
    private GameObject tpArea;
    private GenerateWorld gWorld = new GenerateWorld();
    private bool goForApi = false;
    private bool goForDataPoints = false;
    
=======
    private GenerateWorld gWorld = new GenerateWorld();
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    public static API_Handler api = new API_Handler();
    public static byte[] osmMapData;
    public static List<Dictionary<string, double>> mapBounds;
    public static List<Dictionary<string, double>> dataPointId;
    public static List<ModelHandle> models; 
    public static List<DataPointInfo> dpInfo = new List<DataPointInfo>();
    public static HistMapObj histMapContainer;
    public static byte[] historyMapData;
    public static Texture historyMap;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
<<<<<<< HEAD
    async void Start()
    {
        tp.SetActive(false);
        tpArea.SetActive(true);

        await APICalls();
        GetDataPointInfo();
        
=======
    void Start()
    {
        APICalls();
        GetDataPointInfo();
        
        models = api.GetModels();
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        if(goForApi && goForDataPoints)
        {
            tp.SetActive(true);
        }
        else
        {
            tp.SetActive(false);
        }
    }


    async 

    Task
APICalls()
    {
        Debug.Log("API Calls being made");
        osmMapData = await Task.Run(() => api.GetOsmMap()); //working
        mapBounds = await Task.Run(() => api.GetMapBounds()); //working
        Debug.Log("Datapoint getting");
        dataPointId = await Task.Run(() => api.GetPointLocations());
        
        models = await Task.Run(() => api.GetModels());
        
=======
        
    }


    private void APICalls()
    {
        osmMapData = api.GetOsmMap(); //working
        mapBounds = api.GetMapBounds(); //working
        dataPointId = api.GetPointLocations(); // working
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
        try
        {
            histMapContainer = api.GetHistMap();
            historyMapData = histMapContainer.MapData;
            historyMap = gWorld.LoadDataIntoTexture(historyMapData);
        }
        catch (Exception e)
        {
            Debug.Log("Historical map not found: " + e);
            // This means there was no historical map in the db
        }
<<<<<<< HEAD
        goForApi = true;
=======

>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="dpc"></param>
<<<<<<< HEAD
    void GetDataPointInfo()
    {
        while (!goForApi)
        {
            Debug.Log("Go for Api " + goForApi.ToString());
            Thread.Sleep(1000);
        }
        
        foreach (var item in dataPointId)
        {
            List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex

            dataPointInformation.Add(api.GetPointInformation(item["id"]));

            DataPointInfo dpi = new DataPointInfo(item["id"],
                dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"],
=======
    private void GetDataPointInfo()
    {

        foreach (var item in dataPointId)
        {
            List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex
            
            dataPointInformation.Add(api.GetPointInformation(item["id"]));

            DataPointInfo dpi = new DataPointInfo(item["id"],
                dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"], 
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
                api.HexStringToBinary(dataPointInformation[0]["point_image"]),
                (float)item["longitude"],
                (float)item["latitude"]);
            dpInfo.Add(dpi);
<<<<<<< HEAD
            Debug.Log("Entered dp info");

        }
                
        goForDataPoints = true;
=======
        }
>>>>>>> 4e83e02852b8e6dc3b11a81d7fc7ea7ae5b4e6fa
    }
}




