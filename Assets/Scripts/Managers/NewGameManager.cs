using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class NewGameManager : MonoBehaviour
{

    public string currentLocation;
    public List<string> visitedLocationsStr;

    private static bool isInitialized = false;

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

    // Inventory
    public PlayerInventory inventory = new PlayerInventory();

    // Diary entries
    private List<DiaryEntry> diaryEntries = new List<DiaryEntry>();

    public IEnumerable<DiaryEntry> DiaryEntries { get { return diaryEntries; } }

    public delegate void OnDiaryEntryAdded(DiaryEntry entry);
    public event OnDiaryEntryAdded onDiaryEntryAdded;

    public LocationMarker CurrentLocationObject
    {
        get
        {
            return LevelInstance.Instance.Diary.LocationMarkerObjects.First(marker => marker.LocationName == currentLocation);
        }
    }

    public static NewGameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            // Detach the child game object.
            transform.SetParent(null, false);
            Instance = this;
            DontDestroyOnLoad(this);
            inventory.Initialize();
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

        InitAfterLoad();
        isInitialized = true;
    }

    private void InitAfterLoad()
    {
        foreach (LocationMarker location in LevelInstance.Instance.Diary.LocationMarkerObjects)
        {
            // Re-callibrating vistedLocarions List
            if (visitedLocationsStr.Contains(location.LocationName) || location.LocationName == currentLocation)
            {
                location.SetUnlocked();
            }

            // Assigning capital markers their art accordingly
            if(location.GetComponent<TransportationButtons>().capital)
            {
                location.GetComponent<Image>().sprite = currentCityCapital;
            }

            foreach(GameObject line in location.GetComponent<TransportationButtons>().availableRoutes)
            {
                line.SetActive(false);
            }
        }

        // Updating Map UI
        for (int i = 0; i < visitedLocationsStr.Count - 1; ++i)
        {
            // Updating Map Markers UI
            LocationMarker visitedMarker = LevelInstance.Instance.Diary.LocationMarkerObjects.First(marker => marker.LocationName == visitedLocationsStr[i]);
            TransportationButtons transportation = visitedMarker.GetComponent<TransportationButtons>();
            bool isCapital = transportation.capital;
            if (visitedLocationsStr[i] == currentLocation)
            {
                visitedMarker.GetComponent<Image>().sprite = isCapital ? currentCityCapital : currentCityMarker;
            }
            else if (isCapital)
            {
                visitedMarker.GetComponent<Image>().sprite = traveledCityCapital;
            }
            else
            {
                visitedMarker.GetComponent<Image>().sprite = traveledCityMarker;
            }

            // Updating Routes UI
            foreach (GameObject line in transportation.availableRoutes)
            {
                line.SetActive(true);
                line.GetComponent<Image>().sprite = line.GetComponent<Route>().untraveledRoute;
                if (line.gameObject.name == visitedLocationsStr[i + 1] || line.gameObject.name == currentLocation)
                {
                    if (i == visitedLocationsStr.Count - 2)
                        line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;
                    else line.GetComponent<Image>().sprite = line.GetComponent<Route>().traveledRoute;
                }
            }
        }

        // Assigning current location to map UI label
        GameObject.FindGameObjectWithTag("CurrentLocation").GetComponent<Text>().text = currentLocation;
    }

    public void PostLevelLoad()
    {
        if(isInitialized)
        {
            InitAfterLoad();
        }
    }

    public void UnlockLocation(string name) 
    {
        LocationMarker marker = LevelInstance.Instance.Diary.LocationMarkerObjects.First(marker => marker.LocationName == name);
        if(marker)
        {
            marker.SetUnlocked();
        }
    }

    public void UnlockAllLocations() 
    {
        foreach(LocationMarker marker in LevelInstance.Instance.Diary.LocationMarkerObjects)
        {
            marker.SetUnlocked();
        }
    }

    public void GoToLocation(string name, string method) 
    {
        Debug.Log("Starting to head down to " + name + " by " + method);
        LocationMarker currentLocationObject = CurrentLocationObject;
        GameObject line = currentLocationObject.GetComponent<TransportationButtons>().availableRoutes.First(route => route.name == name);
        if(line == null)
        {
            return;
        }

        LocationMarker newLocation = LevelInstance.Instance.Diary.LocationMarkerObjects.First(marker => marker.LocationName == name);
        if (newLocation == null)
        {
            return;
        }

        // Initiate loading screen to move to new location

        // Update Map UI
        foreach(GameObject currentLine in currentLocationObject.GetComponent<TransportationButtons>().availableRoutes)
        {
            if (currentLine.GetComponent<Route>().attachedMarker.GetComponent<TransportationButtons>().capital)
                currentLine.GetComponent<Route>().attachedMarker.GetComponent<Image>().sprite = untraveledCityCapital;
            else currentLine.GetComponent<Route>().attachedMarker.GetComponent<Image>().sprite = untraveledCityMarker;
            currentLine.GetComponent<Image>().sprite = currentLine.GetComponent<Route>().untraveledRoute;
            currentLine.SetActive(true);
        }

        //if(method == "Ship")
        //    line.GetComponent<Image>().sprite = line.GetComponent<Route>().waterRoute;
        //else 
        line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;

        // Add all routes to an array to be updated in the next city to be 'traveled'
        if(!visitedLocationsStr.Contains(currentLocation))
        {
            visitedLocationsStr.Add(currentLocation);
        }

        if (currentLocationObject.GetComponent<TransportationButtons>().capital)
            currentLocationObject.GetComponent<Image>().sprite = traveledCityCapital;
        else currentLocationObject.GetComponent<Image>().sprite = traveledCityMarker;

        line.SetActive(true);

        // Set next location variables
        currentLocation = name;
        if (newLocation.GetComponent<TransportationButtons>().capital)
            newLocation.GetComponent<Image>().sprite = currentCityCapital;
        else newLocation.GetComponent<Image>().sprite = currentCityMarker;

        currentLocationObject.GetComponent<TransportationButtons>().DisableTransportationOptions();

        // Load level
        SceneManager.LoadScene(sceneName: "LoadingScene");
    }

    public void AddDiaryEntry(DiaryEntry entry)
    {
        diaryEntries.Add(entry);
        if(onDiaryEntryAdded != null)
        {
            onDiaryEntryAdded.Invoke(entry);
        }
    }
}
