using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGameManager : MonoBehaviour
{

    public string currentLocation;
    public GameObject currentLocationGO;
    public List<GameObject> allLocations;
    public List<GameObject> visitedLocations;

    private static bool isInitialized = false;
    private static GameObject instance;

    // Game Stats
    public float time;
    public int food;
    public int money;

    // UI 
    public Sprite traveledCityMarker;
    public Sprite currentCityMarker;
    public Sprite untraveledCityMarker;

    public Sprite traveledCityCapital;
    public Sprite currentCityCapital;
    public Sprite untraveledCityCapital;

    

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
            Debug.Log(name);
            Debug.Log(location.gameObject.name);
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

                // Update Map UI
                foreach (GameObject line2 in currentLocationGO.GetComponent<TransportationButtons>().availableRoutes) 
                {
                    if (line2.GetComponent<Route>().attachedMarker.GetComponent<TransportationButtons>().capital)
                        line2.GetComponent<Route>().attachedMarker.GetComponent<Image>().sprite = untraveledCityCapital;
                    else line2.GetComponent<Route>().attachedMarker.GetComponent<Image>().sprite = untraveledCityMarker;
                    line2.GetComponent<Image>().sprite = line2.GetComponent<Route>().untraveledRoute;
                    line2.SetActive(true);
                }

                line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;
                // Add all routes to an array to be updated in the next city to be 'traveled'
                currentLocationGO.GetComponent<Image>().sprite = traveledCityMarker;

                line.SetActive(true);

                // Set next location variables
                currentLocation = name;
                currentLocationGO = newLocation;
                if (newLocation.GetComponent<TransportationButtons>().capital)
                    newLocation.GetComponent<Image>().sprite = currentCityCapital;
                else newLocation.GetComponent<Image>().sprite = currentCityMarker;

                currentLocationGO.GetComponent<TransportationButtons>().DisableTransportationOptions();
                // Load level

                Debug.Log("Traveled to " + name + " by " + method);
                

            }
            
        }
    }



}
