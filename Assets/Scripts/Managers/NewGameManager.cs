using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewGameManager : MonoBehaviour
{

    public string currentLocation;
    public GameObject currentLocationGO;
    public List<GameObject> allLocations;
    public List<string> allLocationsStr;
    public List<GameObject> visitedLocations;
    public List<string> visitedLocationsStr;

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

    public static NewGameManager Instance
    {
        get
        {
            return instance?.GetComponent<NewGameManager>();
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (instance == null)
        {
            instance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {

        // ^^^^^^^^^^^^^^^^^^^ Old Manager (for reference); to be deleted by Loai after GM is done ^^^^^^^^^^^^^^^^^^^^
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
        // ^^^^^^^^^^^^^^^^^^^ Old Manager code until here ^^^^^^^^^^^^^^^^^^^^

        // Assigning current (starting) location & making it's marker available according to its type
        foreach (GameObject location in allLocations)
        {

            // Initializing string data collection (Locations, visited locations)
            Debug.Log(location.transform.name.Split(' ')[0]);
            Debug.Log(allLocationsStr);
            Debug.Log(location);
            allLocationsStr.Add(location.transform.name.Split(' ')[0]);

            // Assigning current location
            if(location.gameObject.name == (currentLocation + " Marker")) 
            {
                currentLocationGO = location;
                currentLocationGO.GetComponent<Button>().interactable = true;
            }

            // Assigning capital markers their art accordingly
            if (location.gameObject.GetComponent<TransportationButtons>().capital) 
            {
                location.GetComponent<Image>().sprite = currentCityCapital;
            }
        }
        // Turning off all map routes/lines
        foreach (GameObject location in allLocations)
        {
            foreach (GameObject line in location.GetComponent<TransportationButtons>().availableRoutes) 
            {
                Debug.Log(line.name + " is set off");
                line.SetActive(false);
            }
        }

        // Assigning current location to map UI label
        GameObject.FindGameObjectWithTag("CurrentLocation").GetComponent<Text>().text = currentLocation;

        isInitialized = true;
    }

    private void InitAfterLoad() 
    {
        visitedLocations.Clear();
        allLocations.Clear();

        GameObject allLocationsGO = GameObject.FindGameObjectWithTag("Locations");
        Debug.Log(allLocationsGO);
        if (allLocationsGO) // If Diary exists
        {


            foreach (Transform location in allLocationsGO.transform)
            {
                allLocations.Add(location.gameObject);
                if (location.gameObject.name == currentLocation + " Marker")
                    currentLocationGO = location.gameObject;
            }

            // Turning off all map routes/lines
            foreach (GameObject location in allLocations)
            {
                foreach (GameObject line in location.GetComponent<TransportationButtons>().availableRoutes)
                {
                    Debug.Log(line.name + " is set off");
                    line.SetActive(false);
                }
            }

            // Re-callibrating vistedLocarions List
            foreach (string visitedLocation in visitedLocationsStr)
            {
                foreach (GameObject location in allLocations)
                {
                    if (location.gameObject.name == visitedLocation + " Marker")
                    {
                        visitedLocations.Add(location);
                        location.GetComponent<Button>().interactable=true;
                    }
                }
            }

            // Update routes UI for traveled and current routes
            for(int i=0;i<visitedLocations.Count-1;i++)
            {
                foreach(GameObject line in visitedLocations[i].GetComponent<TransportationButtons>().availableRoutes) 
                {
                    line.SetActive(true);
                    line.GetComponent<Image>().sprite = line.GetComponent<Route>().untraveledRoute;
                    Debug.Log(line.gameObject.name);
                    if (line.gameObject.name == visitedLocations[i + 1].gameObject.name || line.gameObject.name == currentLocation) 
                    {
                        if (i == visitedLocations.Count - 2)
                            line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;
                        else line.GetComponent<Image>().sprite = line.GetComponent<Route>().traveledRoute;
                    }
                }
            }

            // Assigning current location to map UI label
            GameObject.FindGameObjectWithTag("CurrentLocation").GetComponent<Text>().text = currentLocation;

            

            // Still need to update the UI; Markers, Routes, Capitals UI depending on visited/unvisited/current.

            
        }
        else Debug.Log("Diary doesn't exist in this scene");
    }

    public void OnLevelWasLoaded() 
    {
        if (isInitialized)
            InitAfterLoad();
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

    public void UnlockAllLocations() 
    {
        foreach (GameObject location in allLocations)
        {
            location.GetComponent<Button>().interactable = true;
            Debug.Log("Unlocked new location: " + name);
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

                //if(method == "Ship")
                //    line.GetComponent<Image>().sprite = line.GetComponent<Route>().waterRoute;
                //else 
                line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;
                // Add all routes to an array to be updated in the next city to be 'traveled'
                if (!visitedLocations.Exists(o => o == currentLocationGO))
                {
                    visitedLocations.Add(currentLocationGO);
                    visitedLocationsStr.Add(currentLocationGO.gameObject.name.Split(' ')[0]);
                }
                if(currentLocationGO.GetComponent<TransportationButtons>().capital)
                    currentLocationGO.GetComponent<Image>().sprite = traveledCityCapital;
                else currentLocationGO.GetComponent<Image>().sprite = traveledCityMarker;

                line.SetActive(true);

                // Set next location variables
                currentLocation = name;
                currentLocationGO = newLocation;
                visitedLocations.Add(currentLocationGO);
                visitedLocationsStr.Add(currentLocationGO.gameObject.name.Split(' ')[0]);
                if (newLocation.GetComponent<TransportationButtons>().capital)
                    newLocation.GetComponent<Image>().sprite = currentCityCapital;
                else newLocation.GetComponent<Image>().sprite = currentCityMarker;

                currentLocationGO.GetComponent<TransportationButtons>().DisableTransportationOptions();
                // Load level
                SceneManager.LoadScene(sceneName: "LoadingScene");

                Debug.Log("Traveled to " + name + " by " + method);
                

            }
            
        }
    }



}
