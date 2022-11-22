using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum TransporationMethod
{
    Foot,
    Train,
    Ship,
    Carriage
}

public class DiaryEntryData
{
    public DiaryEntry entry;
}

public class Journey
{
    public string destination;
    public TransporationMethod method;
    public int money;
    public DiaryEntryData diaryEntry;
}

public class NewGameManager : MonoBehaviour
{

    public string userName;
    
    public string currentLocation;
    public List<string> visitedLocationsStr;

    private static bool isInitialized = false;
    
    // Game Stats
    public bool gameRunning = true;
    public float timeSpeed = 0.1f;
    public float seconds;
    public int minutes;
    public int hour;
    public int day = 0;

    public Transform hourHandle;
    public Transform minuteHandle;
    public float minuteOffset = 13.193f;
    public float hourOffset = 101.601f;

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

    public PopupManager popups;

    // Inventory
    public PlayerInventory inventory = new PlayerInventory();

    // Diary entries
    private List<DiaryEntry> diaryEntries = new List<DiaryEntry>();
    public DiaryEntry TEST_ParisEntry;

    // Map routes 
    public TransportationInfoTable transportationInfo { get; private set; } = new TransportationInfoTable();
    public TextAsset transportationTableCSV;

    // Conditions
    public DialogConditionProvider conditions = new DialogConditionProvider();

    public IEnumerable<DiaryEntry> DiaryEntries { get { return diaryEntries; } }

    // Quests
    private List<Quest> mainQuests = new List<Quest>();
    private List<Quest> sideQuests = new List<Quest>();
    private List<Quest> finishedMainQuests = new List<Quest>();
    private List<Quest> finishedSideQuests = new List<Quest>();

    public delegate void OnQuestAddedEvent(Quest quest);
    public event OnQuestAddedEvent onQuestAdded;

    public delegate void OnQuestFinishedEvent(Quest quest);
    public event OnQuestFinishedEvent onQuestFinished;

    // Events
    public delegate void OnDiaryEntryAdded(DiaryEntry entry);
    public event OnDiaryEntryAdded onDiaryEntryAdded;

    public delegate void OnFoodChangedDelegate(int food);
    public event OnFoodChangedDelegate onFoodChanged;

    public delegate void OnMoneyChangedDelegate(int money);
    public event OnMoneyChangedDelegate onMoneyChanged;

    public delegate void OnDateChangedDelegate(string date);
    public event OnDateChangedDelegate onDateChanged;

    //public delegate void OnTimeChangedDelegate(float time);
    //public event OnTimeChangedDelegate onTimeChanged;

    public LocationMarker CurrentLocationObject
    {
        get
        {
            return LevelInstance.Instance.IngameDiary.LocationMarkerObjects.First(marker => marker.LocationName == currentLocation);
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
        if (gameRunning)
        {
            if (!minuteHandle || !hourHandle)
            {
                minuteHandle = GameObject.FindGameObjectWithTag("minutesHandle").transform;
                hourHandle = GameObject.FindGameObjectWithTag("hoursHandle").transform;
            }

            seconds += Time.deltaTime * timeSpeed;
            minuteHandle.rotation = Quaternion.Euler(0, 0, minuteHandle.rotation.z - (minutes * (360 / 60)) + minuteOffset);
            hourHandle.rotation = Quaternion.Euler(0, 0, hourHandle.rotation.z - (hour * (360 / 12) + (minutes * 0.5f)) + hourOffset);


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

            if (hour >= 10) 
            {
                popups.OpenEndDayPopUp();
            }
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
        SetMorningTime();

        transportationInfo.Initialize(transportationTableCSV);

        InitAfterLoad();
        isInitialized = true;
    }

    private void InitAfterLoad()
    {
        conditions.ResetLocalConditions();

        foreach (LocationMarker location in LevelInstance.Instance.IngameDiary.LocationMarkerObjects)
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
            LocationMarker visitedMarker = LevelInstance.Instance.IngameDiary.LocationMarkerObjects.First(marker => marker.LocationName == visitedLocationsStr[i]);
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
        LocationMarker marker = LevelInstance.Instance.IngameDiary.LocationMarkerObjects.First(marker => marker.LocationName == name);
        if(marker)
        {
            marker.SetUnlocked();

            // Updating marker UI
            if (marker.gameObject.GetComponent<TransportationButtons>().capital) marker.gameObject.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityCapital; // Discovered capital
            else marker.gameObject.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityMarker; // Discovered marker
            
            // Updating route UI
            foreach (GameObject line in CurrentLocationObject.gameObject.GetComponent<TransportationButtons>().availableRoutes)
            {
                if(line.name == name) {
                    line.SetActive(true);
                    line.GetComponent<Image>().sprite = line.GetComponent<Route>().untraveledRoute;
                }
            }
        }
    }

    public void UnlockAllLocations() 
    {
        foreach(LocationMarker marker in LevelInstance.Instance.IngameDiary.LocationMarkerObjects)
        {
            marker.SetUnlocked();
        }
    }

    private void SetFood(int newFood)
    {
        food = Mathf.Max(newFood, 0);
        onFoodChanged?.Invoke(food);
    }

    public void SetMoney(int newMoney)
    {
        money = newMoney;
        onMoneyChanged?.Invoke(money);
    }

    private void SetDate(string newDate)
    {
        date = newDate;
        onDateChanged?.Invoke(date);
    }

    public void SleepOutside() { }

    public void SleepInHostel(int cost, int motherFoodAmount, int boyFoodAmount, int girlFoodAmount) {
        money -= cost;
        
    }
    public void StartNewDay() 
    {
        Debug.Log("Go to next day here, via clock");
        day += 1;
        SetMorningTime();
    }

    public void SetMorningTime() 
    {
        hour = 0;
        minutes = 0;
    }

    // I commented this as it gave me errors - L
    /*private void SetTime(float newTime)
    {
        time = newTime;
        onTimeChanged?.Invoke(time);
    }*/

    public void GoToLocation(string name, string method)
    {
        TransportationRouteInfo routeInfo = transportationInfo.GetRouteInfo(currentLocation, name, method);

        if (routeInfo == null) return;
        float timeNeeded = routeInfo.time;
        int moneyNeeded = routeInfo.cost;
        int foodNeeded = routeInfo.food;


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

        LocationMarker newLocation = LevelInstance.Instance.IngameDiary.LocationMarkerObjects.First(marker => marker.LocationName == name);
        if (newLocation == null)
        {
            return;
        }

        // Initiate loading screen to move to new location

        // Update Map UI
        foreach(GameObject currentLine in currentLocationObject.GetComponent<TransportationButtons>().availableRoutes)
        {
            if (currentLine.GetComponent<Route>().attachedMarker.GetComponent<TransportationButtons>().capital)
                currentLine.GetComponent<Route>().attachedMarker.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityCapital;
            else currentLine.GetComponent<Route>().attachedMarker.transform.GetChild(2).GetComponent<Image>().sprite = untraveledCityMarker;
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
            currentLocationObject.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityCapital;
        else currentLocationObject.transform.GetChild(2).GetComponent<Image>().sprite = traveledCityMarker;

        line.SetActive(true);

        // Set next location variables
        currentLocation = name;
        if (newLocation.GetComponent<TransportationButtons>().capital)
            newLocation.transform.GetChild(2).GetComponent<Image>().sprite = currentCityCapital;
        else newLocation.transform.GetChild(2).GetComponent<Image>().sprite = currentCityMarker;

        currentLocationObject.GetComponent<TransportationButtons>().DisableTransportationOptions();

        // Load level
        AudioManager.Instance.FadeOutMusic();
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

    public void GeneratePDF()
    {
        Debug.Log("Generating PDF");
        PDFBuilder builder = new PDFBuilder();
        builder.Generate(new DiaryEntryData { entry = TEST_ParisEntry });
        Debug.Log("Finished PDF generating");
    }

    public void SetPaused(bool paused)
    {
        gameRunning = !paused;
    }

#if UNITY_EDITOR
    private void ValidateQuest(Quest quest)
    {
        if(string.IsNullOrWhiteSpace(quest.Id))
        {
            Debug.LogError($"{quest.name} has no id");
        }

        if(quest.Title == null || quest.Title.IsEmpty)
        {
            Debug.LogError($"{quest.name} has no title set");
        }
    }
#endif

    public bool AddQuest(Quest quest)
    {
#if UNITY_EDITOR
        ValidateQuest(quest);
#endif

        if(HasQuest(quest))
        {
            return false;
        }

        switch(quest.Type)
        {
            case QuestType.MainQuest:
                mainQuests.Add(quest);
                break;

            case QuestType.SideQuest:
                sideQuests.Add(quest);
                break;
        }

        OnQuestAdded(quest);
        return true;
    }

    private void OnQuestAdded(Quest quest)
    {
        Debug.Log($"Quest {quest.Id} was added");
        onQuestAdded?.Invoke(quest);

        if(EvaluateQuestFinishedCondition(quest))
        {
            // Quest is already finished
            FinishQuest(quest);
        }
        else
        {
            // Listen to changes to conditions.
            conditions.AddOnConditionsChanged(quest.FinishedCondition.GetAllConditions(), OnQuestConditionsChanged, quest);
        }
    }

    private void OnQuestConditionsChanged(object context)
    {
        Quest quest = (Quest)context;
        if(EvaluateQuestFinishedCondition(quest))
        {
            FinishQuest(quest);
        }
    }

    public void FinishQuest(Quest quest)
    {
        GetQuestList(quest.Type).Remove(quest);
        GetQuestList(quest.Type, true).Add(quest);
        OnQuestFinished(quest);
    }

    private void OnQuestFinished(Quest quest)
    {
        Debug.Log($"Quest {quest.Id} was finished");
        onQuestFinished?.Invoke(quest);
    }

    private bool EvaluateQuestFinishedCondition(Quest quest)
    {
        return quest.FinishedCondition.Test();
    }

    public bool HasQuest(Quest quest, bool includeFinished = false)
    {
        return IsQuestActive(quest) || (includeFinished && IsQuestFinished(quest));
    }

    public bool IsQuestActive(Quest quest)
    {
        return GetQuestList(quest.Type).Contains(quest);
    }

    public bool IsQuestFinished(Quest quest)
    {
        return GetQuestList(quest.Type, true).Contains(quest);
    }

    private List<Quest> GetQuestList(QuestType type, bool finished = false)
    {
        switch(type)
        {
            case QuestType.SideQuest:
                return finished ? finishedSideQuests : sideQuests;

            case QuestType.MainQuest:
                return finished ? finishedMainQuests : mainQuests;
        }

        return null;
    }
}
