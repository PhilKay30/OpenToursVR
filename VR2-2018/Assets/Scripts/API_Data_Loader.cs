using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class API_Data_Loader : MonoBehaviour
{
    private GenerateWorld gWorld = new GenerateWorld();
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
    void Start()
    {
        APICalls();
        GetDataPointInfo(GenerateWorld.dpc);
        List<ModelHandle> models = api.GetModels();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void APICalls()
    {
        osmMapData = api.GetOsmMap(); //working
        mapBounds = api.GetMapBounds(); //working
        dataPointId = api.GetPointLocations(); // working
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

    }




    private void GetDataPointInfo(List<DataPointContainer> dpc)
    {
        foreach (var item in dpc)
        {
            List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex

            dataPointInformation.Add(api.GetPointInformation(item.Id));
            DataPointInfo dpi = new DataPointInfo(item.Id, dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"], api.HexStringToBinary(dataPointInformation[0]["point_image"]));
            dpInfo.Add(dpi);
        }
    }
}
