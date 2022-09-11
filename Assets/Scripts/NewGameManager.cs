using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGameManager : MonoBehaviour
{

    public string currentLocation;
    public GameObject currentLocationGO;
    public List<GameObject> allLocations;

    private static bool isInitialized = false;
    private static GameObject instance;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        if (instance == null)
        {
            instance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        }

        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        //map = WorldMapGlobe.instance;

        /*CityManager = new CityMarker(map);
        NavigationMarker = new NavigationMarker(map);
        TransportationManager = new TransportationManager();
        GlobeDesigner = new GlobeDesigner(map);

        StateManager.CurrentState.FreezeTime = true;
        CityManager.DrawLabel(CityData.Pfaffenthal);

        GlobeDesigner.AssignTextures();

        InitializeMissingCities();
        SetCurrentCityText();
        Subscribe();
        map.DrawCity(CityData.Pfaffenthal);
        Navigate();*/

        foreach (GameObject location in allLocations)
        {
            if(location.gameObject.name == (currentLocation + " Marker")) 
            {
                currentLocationGO = location;
                currentLocationGO.GetComponent<Button>().interactable = true;
            }
        }
        
        isInitialized = true;
    }

    public void UnlockLocation(string name) 
    {
        foreach(GameObject location in allLocations) 
        {
            if (location.gameObject.name == (name + " Marker")) {
                location.GetComponent<Button>().interactable = true;
                Debug.Log("Unlocked new location: " + name);
            }
        }
    }

    public void GoToLocation(string name, string method) 
    {
        Debug.Log("Starting to head down to " + name + " by " + method);
        GameObject newLocation = null;
        foreach(GameObject location in allLocations) 
        {
            if (location.name == (name + " Marker"))
                newLocation = location;
        }
        foreach(GameObject line in currentLocationGO.GetComponent<TransportationButtons>().availableRoutes) 
        {
            if(line.name == name) 
            {
                // Initiate loading screen to move to new location
                line.SetActive(true);

                // Set next location variables
                currentLocation = name;
                currentLocationGO = newLocation;
                currentLocationGO.GetComponent<TransportationButtons>().DisableTransportationOptions();
                // Load level

                Debug.Log("Traveled to " + name + " by " + method);
                

            }
        }
    }



}
