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
    public float timeSpeed = 0.1f;
    public float seconds;
    public int minutes;
    public int hour;

    public int food;
    public int money;
    public string date;

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

    public delegate void OnFoodChangedDelegate(int food);
    public event OnFoodChangedDelegate onFoodChanged;

    public delegate void OnMoneyChangedDelegate(int money);
    public event OnMoneyChangedDelegate onMoneyChanged;

    public delegate void OnDateChangedDelegate(string date);
    public event OnDateChangedDelegate onDateChanged;

    public delegate void OnTimeChangedDelegate(float time);
    public event OnTimeChangedDelegate onTimeChanged;

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
            if(string.IsNullOrWhiteSpace(currentLocation))
            {
                currentLocation = SceneManager.GetActiveScene().name;
            }

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

    void Update() 
    {
        seconds += Time.deltaTime * timeSpeed;

        if (seconds >= 60) 
        {
            seconds = 0;
            minutes += 1;
        }

        if (minutes >= 60) 
        {
            hour += 1;
            minutes = 0;
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
                location.transform.GetChild(2).GetComponent<Image>().sprite = currentCityCapital;
            }

            foreach(GameObject line in location.GetComponent<TransportationButtons>().availableRoutes)
            {
                line.SetActive(false);
            }
        }

        // Updating Map UI
        for (int i = 0; i < visitedLocationsStr.Count; ++i)
        {
            // Updating Map Markers UI
            LocationMarker visitedMarker = LevelInstance.Instance.Diary.LocationMarkerObjects.First(marker => marker.LocationName == visitedLocationsStr[i]);
            TransportationButtons transportation = visitedMarker.GetComponent<TransportationButtons>();
            bool isCapital = transportation.capital;
            if (visitedLocationsStr[i] == currentLocation)
            {
                visitedMarker.transform.GetChild(2).GetComponent<Image>().sprite = isCapital ? currentCityCapital : currentCityMarker;
            }
            else if (isCapital)
            {
                visitedMarker.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityCapital;
            }
            else
            {
                visitedMarker.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityMarker;
            }

            // Updating Routes UI
            foreach (GameObject line in transportation.availableRoutes)
            {
                line.SetActive(true);
                if(line.gameObject.name == currentLocation && i == visitedLocationsStr.Count - 1)
                {
                    line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;
                }
                else if(i < visitedLocationsStr.Count - 1 && line.gameObject.name == visitedLocationsStr[i + 1])
                {
                    line.GetComponent<Image>().sprite = line.GetComponent<Route>().traveledRoute;
                }
                else
                {
                    line.GetComponent<Image>().sprite = line.GetComponent<Route>().untraveledRoute;
                }
            }
        }
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

    private void SetFood(int newFood)
    {
        food = Mathf.Max(newFood, 0);
        onFoodChanged?.Invoke(food);
    }

    private void SetMoney(int newMoney)
    {
        money = newMoney;
        onMoneyChanged?.Invoke(money);
    }

    private void SetDate(string newDate)
    {
        date = newDate;
        onDateChanged?.Invoke(date);
    }

    // I commented this as it gave me errors - L
    /*private void SetTime(float newTime)
    {
        time = newTime;
        onTimeChanged?.Invoke(time);
    }*/

    public void GoToLocation(string name, string method, float timeNeeded, int moneyNeeded, int foodNeeded) 
    {
        if (money < moneyNeeded || food < foodNeeded)
        {
            return;
        }

        // Consuming money and food accordingly
        SetFood(food - foodNeeded);
        SetMoney(money - moneyNeeded);
        //SetTime(time + timeNeeded); // Have to uncomment this later on when time is fixed - L

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
        /*foreach(GameObject currentLine in currentLocationObject.GetComponent<TransportationButtons>().availableRoutes)
        {
            if (currentLine.GetComponent<Route>().attachedMarker.GetComponent<TransportationButtons>().capital)
                currentLine.GetComponent<Route>().attachedMarker.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityCapital;
            else currentLine.GetComponent<Route>().attachedMarker.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityMarker;
            currentLine.GetComponent<Image>().sprite = currentLine.GetComponent<Route>().untraveledRoute;
            currentLine.SetActive(true);
        }


        line.GetComponent<Image>().sprite = line.GetComponent<Route>().currentRoute;*/ // Loai: Commented update UI so UI always updates after scene load.

        // Add all routes to an array to be updated in the next city to be 'traveled'
        if(!visitedLocationsStr.Contains(currentLocation))
        {
            visitedLocationsStr.Add(currentLocation);
        }

        /*if (currentLocationObject.GetComponent<TransportationButtons>().capital)
            currentLocationObject.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityCapital;
        else currentLocationObject.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityMarker;

        line.SetActive(true);*/ // Loai: Commented update UI so UI always updates after scene load.

        // Set next location variables
        currentLocation = name;
        /*if (newLocation.GetComponent<TransportationButtons>().capital)
            newLocation.transform.GetChild(2).GetComponent<Image>().sprite = currentCityCapital;
        else newLocation.transform.GetChild(2).GetComponent<Image>().sprite = currentCityMarker;*/ // Loai: Commented update UI so UI always updates after scene load.

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
