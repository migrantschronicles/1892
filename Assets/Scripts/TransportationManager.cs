using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPM;

public class TransportationManager : MonoBehaviour
{

    public WorldMapGlobe map;

    // public string[] availableCities;
    // public Dictionary <string,string[]> transportationMethods;

    [System.Serializable]
    public struct PathsDetails
    {
        public string method;
        public string path;
        // public float time; // Seperate it
        // public float distance; // Seperate it
        // public float cost; // Seperate it
    }

    [System.Serializable]
    public struct TransportationMethods
    {
        public string originCity;
        public string destinationCity;
        public PathsDetails[] methodsAndTime;
    }
    public TransportationMethods[] availableCities;

    // Start is called before the first frame update
    void Start()
    {
        // string[] cityNames = map.GetCityNames();
        // for(int i=0;i<cityNames.Length;i++)
        //     Debug.Log(cityNames[i]);

        // Cell[] all_cells = map.cells;
        // for(int i=0;i<all_cells.Length;i++)
        //     Debug.Log(all_cells[i]);

        // LuxIndex = map.GetCountryIndex("Luxembourg");
        // AmericaIndex = map.GetCountryIndex("United States of America");
        // map.FlyToCountry(AmericaIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetAllTransports()
    {

    }

    public void GetTransports(int origin, int destination)
    {
        // map.FindPath(map.GetCellIndex(localPosition), cell2);
    }
}
