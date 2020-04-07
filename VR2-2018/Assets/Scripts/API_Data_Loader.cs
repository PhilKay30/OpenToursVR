using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class API_Data_Loader : MonoBehaviour
{
    [SerializeField]
    private GameObject tp;
    [SerializeField]
    private GameObject tpArea;
    private GenerateWorld gWorld = new GenerateWorld();
    private bool goForApi = false;
    private bool goForDataPoints = false;
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

    async void Start()
    {
        tp.SetActive(false);
        tpArea.SetActive(true);

        await APICalls();
        GetDataPointInfo();
    }

    // Update is called once per frame
    void Update()
    {
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
        goForApi = true;

    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="dpc"></param>
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
                api.HexStringToBinary(dataPointInformation[0]["point_image"]),
                (float)item["longitude"],
                (float)item["latitude"]);
            dpInfo.Add(dpi);
            Debug.Log("Entered dp info");

        }
                
        goForDataPoints = true;
    }
}




