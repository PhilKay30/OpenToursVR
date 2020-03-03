/*
* NAME      :  ColourToPrefab 
* PURPOSE   :  This helper class to make it easy in unity to map a colour to a prefab object
*/
using UnityEngine;
namespace Assets.Scripts
{
    [System.Serializable]
    public class ColourToPrefab
    {
        public Color colour;
        public GameObject prefab;
    }
}
