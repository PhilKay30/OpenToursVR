using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TourPanel : MonoBehaviour
{
    private API_Handler api = new API_Handler();
    private GenerateWorld gWorld = new GenerateWorld();
    
    private string openingText = "Welcome to OpenToursVR " +
        "To navigate look in the direction you wish to go push " +
        "forward on the left analog stick and let go. To make " +
        "me disapear press down on the left analog stick";

    public TextMeshProUGUI tourInfoText;
    public Image tourImage;
    public GameObject player;
    public List<DataPointInfo> dpInfo = new List<DataPointInfo>();

   
    // Start is called before the first frame update
    void Start()
    {
        tourInfoText.text = openingText;
        
        //https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        Texture2D texture = LoadPNG(Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\controller.png");
        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        mat.mainTexture = texture;
        tourImage.material = mat;
        //GetDataPointInfo(GenerateWorld.dpc);
        
    }

    /*
    private void GetDataPointInfo(List<DataPointContainer> dpc)
    {
        foreach (var item in dpc)
        {
            List<Dictionary<string, string>> dataPointInformation = new List<Dictionary<string, string>>(); /// Will hold point name, description and image hex

            dataPointInformation.Add(api.GetPointInformation(item.Id));
            DataPointInfo dpi = new DataPointInfo(item.Id, dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"], api.HexStringToBinary(dataPointInformation[0]["point_image"]));
            dpInfo.Add(dpi);
            
            //tourInfoText.text = dataPointInformation[0]["point_name"] + "\n" + dataPointInformation[0]["point_desc"];
            //Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            //Texture2D tex = gWorld.LoadDataIntoTexture(api.HexStringToBinary(dataPointInformation[0]["point_image"]));
            //mat.mainTexture = tex;
            //tourImage.material = mat;
        }
    }
    */

    // Update is called once per frame
    void Update()
    {
        /// Here we get the player's position
        /// It changes every time with a variance of about 1.2
        /// The idea is to compare the player's X position to the 
        /// POI's X position.  But the variance will cause issues
        Vector3 playerPos = player.transform.position;


        foreach (var item in API_Data_Loader.dpInfo)
        {
            Debug.Log("Player x: " + playerPos.x);
            Debug.Log("POI x: " + item.PoiLocation.x);
            Debug.Log("Difference: " + (item.PoiLocation.x - playerPos.x));

            if (CheckPosition(playerPos.x, item.PoiLocation.x))
            {
                var IcantThinkOfANameForThis = API_Data_Loader.dpInfo.Find(x => x.Id == item.Id);
                tourInfoText.text = IcantThinkOfANameForThis.PointDescription;
                tourImage.material = IcantThinkOfANameForThis.ImageMaterial;
            }
        }
    }


    /// <summary>
    /// Floating point math, great
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="poiLocation"></param>
    /// <returns></returns>
    private bool CheckPosition(float playerPos, float poiLocation)
    {
        float tollerance = 1.2f;
        float difference = poiLocation - playerPos;
        difference = Math.Abs(difference);
        // https://stackoverflow.com/questions/3188672/how-to-elegantly-check-if-a-number-is-within-a-range
        // but that work only for Int, thanks for nothing LINQ
        // https://stackoverflow.com/questions/42906439/check-if-a-value-exists-between-two-numbers-float-c-sharp
        if (difference <= tollerance)
        {
            return true;
        }
        else
        {
            return false;
        }

    }





    public Texture2D LoadPNG(string filePath)
    {
        Debug.Log("Dir: " + Directory.GetCurrentDirectory().ToString());
        Debug.Log("filePath: " + filePath);
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            Debug.Log("File exists");
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}





public class DataPointInfo
{
    private GenerateWorld gWorld = new GenerateWorld();
    public string PointDescription { set; get; }
    public Texture2D PointImage { set; get; }
    public double Id { set; get; }
    public float Longitude { set; get; }
    public float Latitude { set; get; }
    public Material ImageMaterial = new Material(Shader.Find("Transparent/Diffuse"));
    public Vector3 PoiLocation { set; get; }

    public DataPointInfo(double id, string description, byte[] texture, float lng, float lat)
    {
        Id = id;
        PointDescription = description;
        PointImage = gWorld.LoadDataIntoTexture(texture);
        ImageMaterial.mainTexture = PointImage;
        Longitude = lng;
        Latitude = lat;
    }

    public string GetTourInfo()
    {
        return PointDescription;
    }
}
