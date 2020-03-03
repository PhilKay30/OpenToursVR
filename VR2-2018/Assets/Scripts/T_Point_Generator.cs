/*
* Project			: 	Capstone
* File				: 	T_Point_Generator.cs
* Programmer		: 	Wheeping Angels
* First Version		: 	January 2020
* Description		:	This will generate teleport Points locations based
*                       on a preloaded tilemap
*                       
*                       TODO: Load the map from file instead of unity
*/
using Assets.Scripts;
using System.IO;
using UnityEngine;


public class T_Point_Generator : MonoBehaviour
{
    public ColourToPrefab[] colourMappings; // holds the colours of what to where to generate

    private Texture2D teleportMap; // holds the map

    //Get this from Db
    private string teleportMapFile = Directory.GetCurrentDirectory() + "\\Assets\\Materials\\Pictures\\32x32Marked.png";


    // Start is called before the first frame update
    void Start()
    {
        var loadFile = gameObject.GetComponent<GenerateWorld>();
        teleportMap = loadFile.LoadMaps(teleportMapFile);
        GenerateLevel();
    }




    /*
    * Function     :	GenerateLevel
    * Description  :    This will generate the points based on the tileMap
    * Parameters   :    none
    * Return Value :    none	
    */
    private void GenerateLevel()
    {
        // Loop through the map's width and heigth
        for(int width = 0; width < teleportMap.width; width++)
        {
            for(int heigth = 0; heigth <teleportMap.height; heigth++)
            {
                GeneratePoint(width, heigth);
            }
        }
    }




    /*
    * Function     :    GeneratePoint	
    * Description  :    This will instantiate the Teleport point.
    *                   The coordinate system works on 0,0 being the left bottom corner
    * Parameters   :    int posX: The X position 
    *                   int posY: The Y position
    * Return Value :    none
    */
    private void GeneratePoint(int posX, int PosY)
    {
        Color pixelColour = teleportMap.GetPixel(posX, PosY);

        if (pixelColour.a == 0) // not transparent
        {
            return; // Ignore transparent
        }

        foreach (ColourToPrefab colourMapping in colourMappings)
        {
            if (colourMapping.colour.Equals(pixelColour))
            {
                Instantiate(colourMapping.prefab, new Vector3(posX, 0, PosY), Quaternion.identity); 
            }
        }
    }
}
