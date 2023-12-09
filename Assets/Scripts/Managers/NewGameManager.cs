using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using System;
using Articy.TheMigrantsChronicles.GlobalVariables;

public enum TransportationMethod
{
    None,
    Walking,
    Tram,
    Carriage,
    Cart,
    Ship,
    Train
}

public class Journey
{
    public string destination;
    public TransportationMethod method;
    public int money;
    public List<DiaryEntryData> diaryEntries = new();
}

public enum StolenItemType
{
    Item,
    Money
}

public class StolenItemInfo
{
    public StolenItemType type;
    public Item item;
    public int money;
}

public enum Currency
{
    Franc,
    Dollar
}

public enum LocationDiscoveryStatus
{
    Undiscovered,
    Discovered,
    Traveled,
    Current
}

public enum SleepMethod
{
    None,
    Hotel,
    Outside,
    Ship
}

/**
 * Holds every data that is persistent between level loads.
 * Is instantiated once the first city loads and persists until the end of the game.
 * Some functionality / data storage is outsourced to individual components, that reside on the same game object that won't be destroyed.
 */
public class NewGameManager : MonoBehaviour
{

    public string userName;
    public bool isHistoryMode = false;
    
    private static bool isInitialized = false;

    private List<Journey> journeys = new();
    public string nextLocation { get; private set; }
    public TransportationMethod nextMethod { get; private set; }
    public TransportationMethod lastMethod { get; private set; } = TransportationMethod.None;
    public ShipManager ShipManager { get { return GetComponent<ShipManager>(); } }

    public delegate void OnRouteDiscoveredEvent(string from, string to, TransportationMethod method);
    public event OnRouteDiscoveredEvent OnRouteDiscovered;

    public LocationManager LocationManager { get { return GetComponent<LocationManager>(); } }
    public RouteManager RouteManager { get { return GetComponent<RouteManager>();  } }

    // Game Stats
    public bool wantsEndOfDay = false;
    public bool wantsEndGame = false;
    public bool wantsToTravel = false;
    public string wantsToTravelTo;
    public TransportationMethod wantsToTravelMethod;

    private float playtime = 0.0f; 
    public int money;
    // Date
    public DateTime date = new DateTime(1892, 7, 27);
    private int travelCountToday = 0;

    // Saved stats 
    public SleepMethod LastSleepMethod { get; private set; }
    public List<StolenItemInfo> LastStolenItems { get; private set; }

    // Inventory
    public PlayerInventory inventory = new PlayerInventory();
    public ItemCategory foodCategory;
    public Item foodItem;
    public ItemManager ItemManager { get { return GetComponent<ItemManager>(); } }

    // Map routes 
    public TransportationInfoTable transportationInfo { get; private set; } = new TransportationInfoTable();
    public TextAsset transportationTableCSV;

    // Conditions
    public DialogConditionProvider conditions { get { return GetComponent<DialogConditionProvider>(); } }

    public DiaryEntryManager DiaryEntryManager { get { return GetComponent<DiaryEntryManager>(); } }
    public TransportationManager TransportationManager { get { return GetComponent<TransportationManager>(); } }

    public GeoJSONManager GeoJSONManager { get { return GetComponent<GeoJSONManager>(); } }
    public QuestManager QuestManager { get { return GetComponent<QuestManager>(); } }
    public CharacterManager CharacterManager { get { return GetComponent<CharacterManager>(); } }
    public TutorialManager TutorialManager { get {  return GetComponent<TutorialManager>(); } }

    // Stealing
    [Tooltip("The probability (weight) that money can be stolen")]
    public float moneyStolenProbabilityWeight = 5.0f;
    [Tooltip("The probability (weight) of each amount of money within the range to be stolen")]
    public AnimationCurve moneyStolenProbabilityCurve;
    [Tooltip("The range of money that can be stolen")]
    public Vector2Int moneyStolenAmountRange;
    [Tooltip("The probability (weight) of each amount of stolen items to be actually stolen")]
    public AnimationCurve stolenAmountProbabilityCurve;
    [Tooltip("The range of item amount that can be stolen")]
    public Vector2Int stolenAmountRange;

    // Dialog languages
    [Tooltip("A random amount of this range of words is omitted when estranging the text")]
    public Vector2Int estrangeWordsOmitCount = new Vector2Int(1, 5);
    [Tooltip("The probability that words from the start will be omitted")]
    public float omitWordsFromStartProbability = 0.5f;

    // Health
    public HealthStatus HealthStatus { get { return GetComponent<HealthStatus>(); } }

    public IEnumerable<DiaryEntryData> DiaryEntries
    { 
        get 
        { 
            return journeys.Select(journey => journey.diaryEntries).SelectMany(entries => entries); 
        }
    }

    // Stats
    public int DaysInCity { get; private set; }

    // Currency
    public Currency CurrentCurrency { get; private set; } = Currency.Franc;
    public delegate void OnCurrencyChangedEvent(Currency currency);
    public event OnCurrencyChangedEvent onCurrencyChanged;

    // Events
    public delegate void OnDiaryEntryAdded(DiaryEntryData entry);
    public event OnDiaryEntryAdded onDiaryEntryAdded;

    public delegate void OnMoneyChangedDelegate(int money);
    public event OnMoneyChangedDelegate onMoneyChanged;

    public delegate void OnDateChangedDelegate(DateTime date);
    public event OnDateChangedDelegate onDateChanged;

    public delegate void OnNewDayDelegate();
    public event OnNewDayDelegate onNewDay;

    //public delegate void OnTimeChangedDelegate(float time);
    //public event OnTimeChangedDelegate onTimeChanged;

    public PlayableCharacterData TEST_PlayableCharacter;
    public PlayableCharacterData PlayableCharacterData { get { return TEST_PlayableCharacter; } }

    public delegate void OnLocationChanged(string location);
    public event OnLocationChanged onLocationChanged;

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
            transportationInfo.Initialize(transportationTableCSV);
            HealthStatus.Init(PlayableCharacterData.protagonistData);
            conditions.Init();
            money = PlayableCharacterData.startMoney;

            Journey journey = new Journey();
            journey.destination = SceneManager.GetActiveScene().name;
            journey.method = TransportationMethod.None;
            journey.money = money;
            journeys.Add(journey);
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;   
    }

    private void OnDestroy()
    {
        if(LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    void Update() 
    {
        playtime += Time.deltaTime;

        if(wantsEndOfDay && CanEndDay())
        {
            EndDay();
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nextLocation)
        {
            // Else it is a special map (ship, stopover).
            OnBeforeSceneActivation();
        }
        else if (scene.name == "Ship")
        {
            if(nextLocation == "NewYorkCity" && nextMethod == TransportationMethod.Ship)
            {
                OnLoadedShip();
            }
            // Else the ship is loaded via save game
        }
        else if (scene.name == ShipManager.StopoverLocation)
        {
            OnLoadedStopover();
        }
        else if (scene.name == "ElisIsland")
        {
            if (nextLocation == "NewYorkCity" && nextMethod == TransportationMethod.Ship)
            {
                OnLoadedElisIsland();
            }
            // Else ElisIsland is loaded via save game
        }

        onLocationChanged?.Invoke(scene.name);
    }

    private bool CanEndDay()
    {
        return LevelInstance.Instance.Mode == Mode.None;
    }
    
    private void EndDay()
    {
        LevelInstance.Instance.OpenEndDayPopup();
        wantsEndOfDay = false;
    }

    private void Initialize()
    {
        // Add main quest
        if(PlayableCharacterData.mainQuest)
        {
            QuestManager.AddQuest(PlayableCharacterData.mainQuest);
        }

        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            AudioManager.Instance.MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            AudioManager.Instance.SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
        }

        InitAfterLoad();
        isInitialized = true;
    }

    private void InitAfterLoad()
    {
        conditions.ResetLocalConditions();
    }

    public void PostLevelLoad()
    {
        if(isInitialized)
        {
            InitAfterLoad();
        }
    }

    public void InitNewGame(string username, bool historyMode)
    {
        userName = username;
        InitHistoryMode(historyMode);
    }

    private void InitHistoryMode(bool historyMode)
    {
        isHistoryMode = historyMode;
        conditions.InitHistoryMode(isHistoryMode);
    }

    public void LoadFromSaveGame(SaveData saveGame)
    {
        switch(saveGame.levelName)
        {
            case "Ship":
                nextLocation = "NewYorkCity";
                nextMethod = TransportationMethod.Ship;
                SaveDataJourney lastJourney = saveGame.journeys.LastOrDefault();
                ShipManager.StartTravellingInShip(lastJourney != null ? lastJourney.destination : "");
                break;
        }
        ///@todo Handle Ship ( nextLocation to NewYork, nextMethod, ShipManager.STartTravelling)
        userName = saveGame.username;
        playtime = saveGame.playtime;
        lastMethod = saveGame.lastMethod;
        SetMoney(saveGame.money);
        SetDate(saveGame.Date);
        InitHistoryMode(saveGame.isHistoryMode);

        journeys.Clear();
        foreach(SaveDataJourney journey in saveGame.journeys)
        {
            List<DiaryEntryData> diaryEntries = new(journey.diaryEntries.Select(entry => DiaryEntryManager.GenerateEntry(entry)));
            journeys.Add(new Journey
            {
                destination = journey.destination,
                method = journey.method,
                money = journey.money,
                diaryEntries = diaryEntries
            });

            foreach(DiaryEntryData entry in diaryEntries)
            {
                onDiaryEntryAdded?.Invoke(entry);
            }
        }

        foreach(SaveDataItem saveItem in saveGame.items)
        {
            Item item = ItemManager.GetItemById(saveItem.id);
            if(item != null)
            {
                inventory.AddItem(item, saveItem.amount);
            }
            else
            {
                Debug.LogError($"Item {saveItem.id} from savegame not found");
            }
        }

        conditions.LoadFromSaveData(saveGame.conditions);

        foreach(SaveDataQuest saveQuest in saveGame.quests)
        {
            Quest quest = QuestManager.GetQuestById(saveQuest.id);
            if(quest != null)
            {
                QuestManager.AddQuest(quest);
                switch (saveQuest.type)
                {
                    case SaveDataQuest.Type.Finished:
                        QuestManager.FinishQuest(quest);
                        break;

                    case SaveDataQuest.Type.Failed:
                        QuestManager.FailQuest(quest);
                        break;
                }
            }
            else
            {
                Debug.LogError($"Quest {saveQuest.id} from savegame not found");
            }
        }

        HealthStatus.LoadFromSaveData(saveGame.health);

        CurrentCurrency = saveGame.currency;
        onCurrencyChanged?.Invoke(CurrentCurrency);

        foreach(RouteManager.DiscoveredRoute route in RouteManager.Routes)
        {
            if(RouteManager.DiscoverRoute(route.from, route.to, route.method))
            {
                OnRouteDiscovered?.Invoke(route.from, route.to, route.method);
            }
        }

        journeys.Add(new Journey
        {
            destination = saveGame.levelName,
            money = money,
            method = saveGame.lastMethod
        });
        LevelInstance.Instance.OpenNewCityDiaryEntry();

        TutorialManager.Instance.LoadFromSaveGame(saveGame);

        onLocationChanged?.Invoke(saveGame.levelName);
    }

    public void SaveGame(SaveData saveGame)
    {
        saveGame.version = SaveGameVersion.V3_TutorialActionsAdded;
        ///@todo selected character
        saveGame.username = userName;
        saveGame.money = money;
        saveGame.playtime = playtime;
        saveGame.Date = date;
        saveGame.isHistoryMode = isHistoryMode;

        switch(SceneManager.GetActiveScene().name)
        {
            case "ElisIsland":
                saveGame.lastMethod = TransportationMethod.Ship;
                saveGame.levelName = "ElisIsland";
                break;

            case "Ship":
                saveGame.lastMethod = TransportationMethod.None;
                saveGame.levelName = "Ship";
                break;

            default:
                saveGame.lastMethod = nextMethod; // Is still set during travel
                saveGame.levelName = nextLocation; // Is still set during travel
                break;
        }

        saveGame.journeys = new List<SaveDataJourney>(journeys.Select(journey => new SaveDataJourney
        {
            destination = journey.destination,
            method = journey.method,
            money = journey.money,
            diaryEntries = new List<DiaryEntryInfo>(journey.diaryEntries.Select(entry => entry.info))
        }));

        foreach (var item in inventory.Items)
        {
            saveGame.items.Add(new SaveDataItem { id = item.Key.id, amount = item.Value });
        }

        saveGame.conditions = conditions.CreateSaveData();

        foreach (Quest quest in QuestManager.MainQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Started });
        }
        foreach (Quest quest in QuestManager.SideQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Started });
        }
        foreach (Quest quest in QuestManager.FinishedMainQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Finished });
        }
        foreach (Quest quest in QuestManager.FinishedSideQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Finished });
        }
        foreach (Quest quest in QuestManager.FailedMainQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Failed });
        }
        foreach (Quest quest in QuestManager.FailedSideQuests)
        {
            saveGame.quests.Add(new SaveDataQuest { id = quest.Id, type = SaveDataQuest.Type.Failed });
        }
        saveGame.health = HealthStatus.CreateSaveData();
        saveGame.currency = CurrentCurrency;
        saveGame.routes = RouteManager.Routes;
        saveGame.completedTutorialFeatures = TutorialManager.Instance.CompletedFeatures;
    }

    private void OnLanguageChanged(Language language)
    {
        journeys.ForEach(journey => journey.diaryEntries.ForEach(entry => DiaryEntryManager.UpdateDiaryEntry(entry)));
    }

    public void DiscoverRoute(string from, string to, TransportationMethod method)
    {
        if (RouteManager.DiscoverRoute(from, to, method))
        {
            OnRouteDiscovered?.Invoke(from, to, method);
        }
    }

    public void SetMoney(int newMoney)
    {
        money = newMoney;
        onMoneyChanged?.Invoke(money);
    }

    private void SetDate(DateTime newDate)
    {
        DateTimeFormatInfo dateFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        date = newDate;
        onDateChanged?.Invoke(date);
    }

    public List<StolenItemInfo> OnSleepOutside(List<EndOfDayHealthData> endOfDayHealthData)
    {
        int distributedFoodAmount = endOfDayHealthData.Select(data => data.foodAmount).Sum();
        if(distributedFoodAmount > 0)
        {
            inventory.RemoveItemCategory(foodCategory, distributedFoodAmount);
        }
        HealthStatus.OnEndOfDay(endOfDayHealthData);
        StartNewDay();
        List<StolenItemInfo> stolenItems = StealItems();
        LastStolenItems = stolenItems;
        LastSleepMethod = SleepMethod.Outside;
        return stolenItems;
    }

    public void OnSleepInHostel(List<EndOfDayHealthData> endOfDayHealthData, int cost, int boughtFoodAmount)
    {
        int distributedFoodAmount = endOfDayHealthData.Select(data => data.foodAmount).Sum();
        int itemsToAdd = boughtFoodAmount;
        if (distributedFoodAmount > 0)
        {
            int itemsRemoved = inventory.RemoveItemCategory(foodCategory, distributedFoodAmount);
            if(itemsRemoved < distributedFoodAmount)
            {
                itemsToAdd -= distributedFoodAmount - itemsRemoved;
            }
        }
        if(itemsToAdd > 0)
        {
            inventory.AddItem(foodItem, itemsToAdd);
        }
        HealthStatus.OnEndOfDay(endOfDayHealthData);
        LastStolenItems = null;
        LastSleepMethod = SleepMethod.Hotel;
        StartNewDay();

        money -= cost;
        onMoneyChanged?.Invoke(money);
    }

    /**
     * @return True if the player reached the destination, false otherwise
     */
    public bool OnSleepInShip(List<EndOfDayHealthData> endOfDayHealthData)
    {
        HealthStatus.OnEndOfDay(endOfDayHealthData);
        LastStolenItems = null;
        LastSleepMethod = SleepMethod.Ship;
        StartNewDay();

        if(ShipManager.HasReachedDestination)
        {
            // Ship travel finished, arrived in Elis island.
            LevelInstance.Instance.OnShipArrived();
            return true;
        }

        return false;
    }

    public void OnLeaveShip()
    {
        SceneManager.LoadScene("LoadingScene");
    }
    
    public void StartNewDay() 
    {
        ++DaysInCity;
        travelCountToday = 0;
        SetDate(date.AddDays(1));
        Vibrate();
        onNewDay?.Invoke();
    }

    public static void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    public void GoToLocation(string name, TransportationMethod method)
    {
        if (nextLocation != null)
        {
            // Already travelling
            return;
        }

        if(travelCountToday > 0)
        {
            // Cannot travel again today
            LevelInstance.Instance.OnCannotTravelAgainToday();
            return;
        }

        // Check if there are quests that failed.
        List<Quest> failedSideQuests = new(QuestManager.GetFailedSideQuestsIfYouLeave(LevelInstance.Instance.LocationName)
            .Concat(QuestManager.GetFailedSideQuestsIfYouGetTo(name)));
        if(failedSideQuests.Any())
        {
            foreach(Quest quest in failedSideQuests)
            {
                QuestManager.FailQuest(quest);
                LevelInstance.Instance.OnQuestFailed(quest);
            }

            wantsToTravel = true;
            wantsToTravelTo = name;
            wantsToTravelMethod = method;
        }
        else
        {
            GoToLocationImplementation(name, method);
        }
    }

    public void GoToWantingLocation()
    {
        if(wantsToTravel)
        {
            string name = wantsToTravelTo;
            TransportationMethod method = wantsToTravelMethod;
            wantsToTravelTo = "";
            wantsToTravel = false;
            wantsToTravelMethod = TransportationMethod.None;
            GoToLocationImplementation(name, method);
        }
    }

    private void GoToLocationImplementation(string name, TransportationMethod method)
    {
        if (LocationManager.IsFromEuropeToAmerica(LevelInstance.Instance.LocationName, name))
        {
            ShipManager.StartTravellingInShip();

            // Load level
            nextLocation = name;
            nextMethod = method;
            lastMethod = method;
        }
        else
        {
            // Check prerequisits
            if (!CanTravelTo(name, method))
            {
                Debug.Log($"Cannot travel to {name} via {method}");
                return;
            }

            TransportationRouteInfo routeInfo = transportationInfo.GetRouteInfo(LevelInstance.Instance.LocationName, name, method);
            if (routeInfo == null)
            {
                Debug.Log($"No route info found for {name} via {method}");
                return;
            }

            if (money < routeInfo.cost)
            {
                Debug.Log($"Not enough money for {name} via {method}");
                return;
            }

            // Remove required costs.
            SetMoney(money - routeInfo.cost);
            ++travelCountToday;

            // Load level
            nextLocation = name;
            nextMethod = method;
            lastMethod = method;

            // For now only in normal cities
            GetComponent<SaveGameManager>().SaveGame();
        }

        // Reset values
        DaysInCity = 0;
        onNewDay?.Invoke();

        AudioManager.Instance.FadeOutMusic();

        SceneManager.LoadScene("LoadingScene");
    }
    
    /**
     * Called if the player is travelling on ship and wants to visit the stopover location.
     */
    public void VisitStopover()
    {
        if(!ShipManager.IsStopoverDay)
        {
            return;
        }

        ShipManager.WantsToVisitStopover = true;
        SceneManager.LoadScene("LoadingScene");
    }

    /**
     * Called if the player spend the day in the stopover location and now returns to the ship. 
     */
    public void ReturnToShip(bool endOfDay)
    {
        if(!ShipManager.IsStopoverDay)
        {
            return;
        }

        ShipManager.HasVisitedStopover = true;
        if(endOfDay)
        {
            wantsEndOfDay = endOfDay;
        }

        SceneManager.LoadScene("LoadingScene");
    }

    /**
     * Called after another scene is loaded, just before that scene is activated and the loading scene is unloaded.
     */
    public void OnBeforeSceneActivation()
    {
        // Add the journey
        Journey journey = new Journey();
        journey.destination = nextLocation;
        journey.method = nextMethod;
        journey.money = money;
        journeys.Add(journey);

        nextLocation = null;
        nextMethod = TransportationMethod.None;

        StartCoroutine(OpenNewCityDiaryEntryNextFrame());
    }

    private IEnumerator OpenNewCityDiaryEntryNextFrame()
    {
        yield return null;
        LevelInstance.Instance.OpenNewCityDiaryEntry();
    }

    public void OnLoadedShip()
    {
        if(wantsEndOfDay)
        {
            // A hack, since CanEndDay() checks whether the game is running
            StartCoroutine(EndDayNextFrame());
        }
        else if(DaysInCity == 0)
        {
            // Add the journey
            Journey journey = new Journey();
            journey.destination = nextLocation;
            journey.method = nextMethod;
            journey.money = money;
            journeys.Add(journey);

            StartCoroutine(OpenNewCityDiaryEntryNextFrame());
        }
    }

    private IEnumerator EndDayNextFrame()
    {
        yield return null;
        LevelInstance.Instance.OpenEndDayPopup();
        wantsEndOfDay = false;
    }

    public void OnLoadedStopover()
    {
        // Add the journey
        Journey journey = new Journey();
        journey.destination = ShipManager.StopoverLocation;
        journey.method = TransportationMethod.Ship;
        journey.money = money;
        journeys.Add(journey);

        StartCoroutine(OpenNewDayDiaryEntryNextFrame());
    }

    private IEnumerator OpenNewDayDiaryEntryNextFrame()
    {
        yield return null;
        LevelInstance.Instance.OpenNewDayDiaryEntry();
    }

    public void OnLoadedElisIsland()
    {
        GetComponent<SaveGameManager>().SaveGame();

        // Add the journey
        Journey journey = new Journey();
        journey.destination = "ElisIsland";
        journey.method = TransportationMethod.Ship;
        journey.money = money;
        journeys.Add(journey);

        nextLocation = null;
        nextMethod = TransportationMethod.None;
        DaysInCity = 0;
        ShipManager.EndTravellingInShip();

        StartCoroutine(OpenNewCityDiaryEntryNextFrame());
    }

    public void AddDiaryEntry(DiaryEntryData entry)
    {
        journeys[journeys.Count - 1].diaryEntries.Add(entry);

        if(onDiaryEntryAdded != null)
        {
            onDiaryEntryAdded.Invoke(entry);
        }
    }

    public DiaryEntryInfo CollectDiaryEntryInfo(GeneratedDiaryEntryPurpose purpose)
    {
        List<ProtagonistHealthData> characters = new List<ProtagonistHealthData>(NewGameManager.Instance.HealthStatus.Characters);

        List<HealthProblem> newProblems = new();
        for (int i = 0; i < characters.Count; ++i)
        {
            ProtagonistHealthData character = characters[i];
            if (character.CholeraStatus.DaysSick == 1)
            {
                // Got sick
                newProblems.Add(new HealthProblem { character = character.CharacterData, sickness = HealthProblemType.Cholera });
                characters.RemoveAt(i);
                --i;
            }
            else if (character.HomesickessStatus.DaysSick == 1)
            {
                // Got homesick
                newProblems.Add(new HealthProblem {character = character.CharacterData, sickness = HealthProblemType.Homesickness });
                characters.RemoveAt(i);
                --i;
            }
        }

        List<HealthProblem> existingProblems = new();
        for (int i = 0; i < characters.Count; ++i)
        {
            ProtagonistHealthData character = characters[i];
            if (character.CholeraStatus.DaysSick > 1)
            {
                // Still sick
                existingProblems.Add(new HealthProblem {character = character.CharacterData, sickness = HealthProblemType.Cholera });
                characters.RemoveAt(i);
                --i;
            }
            else if (character.HomesickessStatus.DaysSick > 1)
            {
                // Still homesick
                existingProblems.Add(new HealthProblem {character = character.CharacterData, sickness = HealthProblemType.Homesickness });
                characters.RemoveAt(i);
                --i;
            }
        }

        List<DiaryHealthState> healthStates = new();
        foreach(ProtagonistHealthData character in HealthStatus.Characters)
        {
            healthStates.Add(new DiaryHealthState { character = character.CharacterData, healthState = character.HealthState });
        }

        DiaryEntryInfo info = new DiaryEntryInfo
        {
            levelMode = LevelInstance.Instance.LevelMode,
            daysInCity = DaysInCity,
            locationName = LevelInstance.Instance.LocationName,
            isStopoverDay = ShipManager.IsStopoverDay,
            lastSleepMethod = LastSleepMethod,
            lastStolenItems = LastStolenItems,
            hungryCharacters = new List<ProtagonistData>(HealthStatus.GetHungryCharacters().Select(character => character.CharacterData)),
            stopoverLocation = ShipManager.StopoverLocation,
            healthStates = healthStates,
            newHealthProblems = newProblems,
            existingHealthProblems = existingProblems,
            lastTransportationMethod = lastMethod,
            purpose = purpose,
            playtime = playtime
        };
        info.Date = date;
        return info;
    }

    public void GeneratePDF()
    {
        Debug.Log("Generating PDF");
        PDFBuilder builder = new PDFBuilder();
        builder.Generate(journeys, userName, playtime);
        builder.GenerateGeoJSON(journeys);
        Debug.Log("Finished PDF generating");
    }

    /**
     * Steals items from the inventory.
     *
     * The number of items stolen is set randomly and controlled by stolenAmountRange and stolenAmountProbabilityCurve.
     * stolenAmountRange sets the range of number of items that can be stolen.
     * If stolenAmountProbabilityCurve is not set or empty, a random number in the stolenAmountRange (inclusive) is chosen.
     * You can control the probabilities of each number in the range by setting up the stolenAmountProbabilityCurve:
     * For each valid number in the range, the curve is evaluated, and the y-value is considered the probability weight.
     * If you don't want a specific number to be chosen, set that y-value to 0.
     * If you e.g. want a specific number to be more likely to be chosen, set that y-value to a higher value than others.
     * If you want all numbers to have the same probability, set the y-value of all numbers in the range to an equal value (not 0).
     * 
     * Money counts as an item in this case and can be stolen only once.
     * You can set the probability weight (that money is stolen) with moneyStolenProbabilityWeight.
     * If money is stolen, you can control the amount of money that is stolen.
     * Like stolenAmountRange and stolenAmountProbabilityCurve, you can set the range and probability weights of each
     * valid amount number that can be stolen with moneyStolenAmountRange and moneyStolenProbabilityCurve.
     * 
     * @return A list of stolen items (are removed from inventory).
     */
    public List<StolenItemInfo> StealItems()
    {
        List<StolenItemInfo> stolenItems = new List<StolenItemInfo>();
        bool canStealMoney = money > 0;
        float weight = canStealMoney ? moneyStolenProbabilityWeight : 0;
        foreach(KeyValuePair<Item, int> item in inventory.Items)
        {
            weight += item.Key.stolenProbabilityWeight * item.Value;
        }

        // Calculate amount
        int stolenAmount = 0;
        if(stolenAmountProbabilityCurve != null && stolenAmountProbabilityCurve.length > 0)
        {
            // If the curve is set, choose a random amount with the given probabilities.
            float amountWeight = 0.0f;
            for (int i = stolenAmountRange.x; i <= stolenAmountRange.y; ++i)
            {
                float currentAmountWeight = stolenAmountProbabilityCurve.Evaluate(i);
                amountWeight += currentAmountWeight;
            }

            float randomAmountWeight = UnityEngine.Random.value * amountWeight;
            for (int i = stolenAmountRange.x; i <= stolenAmountRange.y; ++i)
            {
                randomAmountWeight -= stolenAmountProbabilityCurve.Evaluate(i);
                if (randomAmountWeight <= 0.0f)
                {
                    stolenAmount = i;
                    break;
                }
            }
        }
        else
        {
            stolenAmount = Mathf.RoundToInt((UnityEngine.Random.value * ((float)stolenAmountRange.y - stolenAmountRange.x)) + stolenAmountRange.x);
        }

        bool wasMoneyStolen = false;
        for(int i = 0; i < stolenAmount && !inventory.IsEmpty; ++i)
        {
            float randomWeight = UnityEngine.Random.value * weight;

            // Check money
            if(!wasMoneyStolen && canStealMoney)
            {
                randomWeight -= moneyStolenProbabilityWeight;
                if (randomWeight <= 0.0f)
                {
                    // Money is stolen
                    int stolenMoneyAmount = 0;
                    if(moneyStolenProbabilityCurve != null && moneyStolenProbabilityCurve.length > 0)
                    {
                        float amountWeight = 0.0f;
                        for (int j = moneyStolenAmountRange.x; j <= moneyStolenAmountRange.y; ++j)
                        {
                            float currentAmountWeight = moneyStolenProbabilityCurve.Evaluate(j);
                            amountWeight += currentAmountWeight;
                        }

                        float randomAmountWeight = UnityEngine.Random.value * amountWeight;
                        for (int j = moneyStolenAmountRange.x; j <= moneyStolenAmountRange.y; ++j)
                        {
                            randomAmountWeight -= moneyStolenProbabilityCurve.Evaluate(j);
                            if (randomAmountWeight <= 0.0f)
                            {
                                stolenMoneyAmount = j;
                                break;
                            }
                        }
                    }
                    else
                    {
                        stolenMoneyAmount = Mathf.RoundToInt((UnityEngine.Random.value * ((float)moneyStolenAmountRange.y - moneyStolenAmountRange.x)) 
                            + moneyStolenAmountRange.x);
                    }

                    stolenMoneyAmount = Mathf.Min(stolenMoneyAmount, money);
                    SetMoney(money - stolenMoneyAmount);
                    StolenItemInfo info = new StolenItemInfo { type = StolenItemType.Money, money = stolenMoneyAmount };
                    stolenItems.Add(info);

                    // Money should not be stolen again.
                    weight -= moneyStolenProbabilityWeight;
                    wasMoneyStolen = true;
                    continue;
                }
            }

            foreach (KeyValuePair<Item, int> item in inventory.Items)
            {
                randomWeight -= item.Key.stolenProbabilityWeight * item.Value;
                if(randomWeight <= 0.0f)
                {
                    // One of those items is stolen, it does not matter which one
                    inventory.RemoveItem(item.Key);
                    weight -= item.Key.stolenProbabilityWeight;
                    StolenItemInfo info = new StolenItemInfo { type = StolenItemType.Item, item = item.Key };
                    stolenItems.Add(info);
                    break;
                }
            }
        }

        // Notify the health status.
        HealthStatus.OnItemsStolen(stolenItems.Count);

        return stolenItems;
    }

    /**
     * @return True if the player character understands the language (e.g. is native or has dictionary).
     */
    public bool UnderstandsDialogLanguage(DialogLanguage language)
    {
        switch(language)
        {
            case DialogLanguage.Native:
                return true;

            case DialogLanguage.English:
                return conditions.HasCondition("english");

            case DialogLanguage.Italian:
                return conditions.HasCondition("italian");
        }

        return false;
    }

    /**
     * Estranges some text (e.g. Hello this is a test -> ... this ...).
     * Is implemented here in case there are differences between the playable characters.
     */
    public string EstrangeText(string text)
    {
        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string result = "";

        int omitWords = 0;
        if(UnityEngine.Random.value < omitWordsFromStartProbability)
        {
            omitWords = UnityEngine.Random.Range(estrangeWordsOmitCount.x, estrangeWordsOmitCount.y);
            result += "???";
        }

        for(int i = 0; i < parts.Length; ++i)
        {
            if(omitWords > 0)
            {
                --omitWords;
                continue;
            }
            else
            {
                if(result.Length > 0)
                {
                    result += " ";
                }

                result += parts[i];
                omitWords = UnityEngine.Random.Range(estrangeWordsOmitCount.x, estrangeWordsOmitCount.y);

                if(i < parts.Length - 1)
                {
                    result += " ???";
                }
            }
        }

        return result;
    }

    public void OnProtagonistDied(ProtagonistData protagonist)
    {
        OnEndOfGame(false);
    }

    public void OnEndOfGame(bool success, string technicalName = "")
    {
        wantsEndGame = true;
        LevelInstance.Instance.OnEndOfGame(success, technicalName);
    }

    public void EndGameAndReturnToMainMenu()
    {
        AudioManager.Instance.StopMusic();
        Destroy(AudioManager.Instance.gameObject);
        Destroy(gameObject);
        GetComponent<SaveGameManager>().OnEndGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void SetCurrency(Currency currency)
    {
        if(currency != CurrentCurrency)
        {
            CurrentCurrency = currency;
            switch(CurrentCurrency)
            {
                case Currency.Franc: money *= 2; break;
                case Currency.Dollar: money /= 2; break;
            }

            onCurrencyChanged?.Invoke(CurrentCurrency);
            onMoneyChanged?.Invoke(money);
        }
    }

    public LocationDiscoveryStatus GetDiscoveryStatus(string location)
    {
        if(location == LevelInstance.Instance.LocationName)
        {
            return LocationDiscoveryStatus.Current;
        }

        foreach(Journey journey in journeys)
        {
            if(journey.destination == location)
            {
                return LocationDiscoveryStatus.Traveled;
            }
        }

        foreach(Journey journey in journeys)
        {
            if(!((ShipManager.HasVisitedStopover || ShipManager.WantsToVisitStopover) && ShipManager.StopoverLocation == journey.destination))
            {
                if (RouteManager.IsRouteDiscovered(journey.destination, location))
                {
                    return LocationDiscoveryStatus.Discovered;
                }
            }
        }

        if(!ShipManager.IsStopoverDay)
        {
            if (RouteManager.IsRouteDiscovered(LevelInstance.Instance.LocationName, location))
            {
                return LocationDiscoveryStatus.Discovered;
            }
        }
        
        return LocationDiscoveryStatus.Undiscovered;
    }

    public LocationDiscoveryStatus GetRouteDiscoveryStatus(string from, string to)
     {
        if(from == LevelInstance.Instance.LocationName)
        {
            if(ShipManager.IsStopoverDay)
            {
                // If it's the stopover day, don't show any routes.
                // If the player stays on the ship, this does not matter anyway.
                return LocationDiscoveryStatus.Undiscovered;
            }
            else
            {
                // If it's a normal day in a city, determine if the route is discovered.
                // Since the player can't go back, the routes from this city can only be discovered.
                if(RouteManager.IsRouteDiscovered(from, to))
                {
                    // The route is discovered
                    return LocationDiscoveryStatus.Discovered;
                }

                // The route is not discovered yet.
                return LocationDiscoveryStatus.Undiscovered;
            }
        }
        else if(to == LevelInstance.Instance.LocationName)
        {
            if(ShipManager.IsStopoverDay)
            {
                // If it's the stopover day, it could be that the player discovered another transportation method.
                return RouteManager.IsRouteDiscovered(from, to) ? LocationDiscoveryStatus.Discovered : LocationDiscoveryStatus.Undiscovered;
            }

            if(journeys.Count > 1 && journeys[journeys.Count - 2].destination == from)
            {
                // This is the route the player came from.
                return LocationDiscoveryStatus.Current;
            }
        }

        for(int i = 0; i < journeys.Count - 1; ++i)
        {
            if (journeys[i].destination == from && journeys[i + 1].destination == to)
            {
                // The player traveled this route
                return LocationDiscoveryStatus.Traveled;
            }
        }

        for(int i = journeys.Count - 1; i >= 0; --i)
        {
            if (journeys[i].destination == from)
            {
                // The route starts from a location where the player was.
                // The route could be discovered by the player or not yet discovered.
                // All other cases are already handled.
                return RouteManager.IsRouteDiscovered(from, to) ? LocationDiscoveryStatus.Discovered : LocationDiscoveryStatus.Undiscovered;
            }
        }

        return LocationDiscoveryStatus.Undiscovered;
    }

    public bool CanTravel(string from, string to, TransportationMethod method = TransportationMethod.None)
    {
          bool hasRouteInfo = transportationInfo.HasRouteInfo(from, to, method);
        if(!hasRouteInfo)
        {
            return false;
        }

        if(method != TransportationMethod.None && !RouteManager.IsRouteDiscovered(from, to, method))
        {
            // Not yet discovered
            return false;
        }

        foreach(var journey in journeys)
        {
            if(journey.destination == to)
            {
                // Cannot travel back.
                return false;
            }
        }

        return true;
    }

    public bool CanTravelTo(string to, TransportationMethod method = TransportationMethod.None)
    {
        return CanTravel(LevelInstance.Instance.LocationName, to, method);
    }

    public static TransportationMethod GetTransportationMethodByName(string method)
    {
        switch(method.ToLower())
        {
            case "walking": return TransportationMethod.Walking;
            case "tram": return TransportationMethod.Tram;
            case "carriage": return TransportationMethod.Carriage;
            case "ship": return TransportationMethod.Ship;
            case "train": return TransportationMethod.Train;
            case "cart": return TransportationMethod.Cart;
            default: Debug.Assert(false, $"Invalid transporation method: {method}"); break;
        }

        return TransportationMethod.Walking;
    }
}
