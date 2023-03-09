using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using System;

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

public class DiaryEntryData
{
    public DiaryEntry entry;
}

public class Journey
{
    public string destination;
    public TransportationMethod method;
    public int money;
    public DiaryEntryData diaryEntry;
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

public class NewGameManager : MonoBehaviour
{

    public string userName;
    
    private static bool isInitialized = false;

    private List<Journey> journeys = new();
    public string nextLocation { get; private set; }
    public TransportationMethod nextMethod { get; private set; }
    public ShipManager ShipManager { get { return GetComponent<ShipManager>(); } }

    public delegate void OnRouteDiscoveredEvent(string from, string to, TransportationMethod method);
    public event OnRouteDiscoveredEvent OnRouteDiscovered;

    public LocationManager LocationManager { get { return GetComponent<LocationManager>(); } }
    public RouteManager RouteManager { get { return GetComponent<RouteManager>();  } }

    // Game Stats
    public bool gameRunning = true;
    public float timeSpeed = 0.1f;
    public int hoursPerDay = 10;
    public float seconds;
    public int minutes;
    public int hour;
    public int day = 0;
    public bool wantsEndOfDay = false;

    public bool IsPaused { get { return !gameRunning; } }

    public delegate void OnPauseChangedEvent(bool paused);
    public event OnPauseChangedEvent OnPauseChanged;

    public int food;
    public int money;
    // Date
    public DateTime date = new DateTime(1892, 6, 21);
    public string dateStr;

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
    public ItemCategory foodCategory;
    public Item foodItem;
    public ItemManager ItemManager { get { return GetComponent<ItemManager>(); } }

    // Diary entries
    private List<DiaryEntry> diaryEntries = new List<DiaryEntry>();
    public DiaryEntry TEST_ParisEntry;

    // Map routes 
    public TransportationInfoTable transportationInfo { get; private set; } = new TransportationInfoTable();
    public TextAsset transportationTableCSV;

    // Conditions
    public DialogConditionProvider conditions { get { return GetComponent<DialogConditionProvider>(); } }

    public IEnumerable<DiaryEntry> DiaryEntries { get { return diaryEntries; } }

    // Quests
    private List<Quest> mainQuests = new List<Quest>();
    private List<Quest> sideQuests = new List<Quest>();
    private List<Quest> finishedMainQuests = new List<Quest>();
    private List<Quest> finishedSideQuests = new List<Quest>();

    public IEnumerable<Quest> MainQuests { get { return mainQuests; } } 
    public IEnumerable<Quest> SideQuests { get { return sideQuests; } }
    public IEnumerable<Quest> FinishedMainQuests { get { return finishedMainQuests; } }
    public IEnumerable<Quest> FinishedSideQuests { get { return finishedSideQuests; } }
    public IEnumerable<Quest> MainQuestsIncludingFinished { get { return mainQuests.Concat(finishedMainQuests); } }
    public IEnumerable<Quest> SideQuestsIncludingFinished { get { return sideQuests.Concat(finishedSideQuests); } }

    public delegate void OnTimeChangedEvent(int hour, int minutes);
    public event OnTimeChangedEvent onTimeChanged;

    public delegate void OnQuestAddedEvent(Quest quest);
    public event OnQuestAddedEvent onQuestAdded;

    public delegate void OnQuestFinishedEvent(Quest quest);
    public event OnQuestFinishedEvent onQuestFinished;

    public Quest TEST_Quest;

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

    // Stats
    public int DaysInCity { get; private set; }

    // Currency
    public Currency CurrentCurrency { get; private set; } = Currency.Franc;
    public delegate void OnCurrencyChangedEvent(Currency currency);
    public event OnCurrencyChangedEvent onCurrencyChanged;

    // Events
    public delegate void OnDiaryEntryAdded(DiaryEntry entry);
    public event OnDiaryEntryAdded onDiaryEntryAdded;

    public delegate void OnFoodChangedDelegate(int food);
    public event OnFoodChangedDelegate onFoodChanged;

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

    public static NewGameManager Instance { get; private set; }

    /**
     * @return The remaining time (in seconds in real time) of the day.
     */
    public float RemainingTime
    {
        get
        {
            if(hour >= hoursPerDay)
            {
                return 0.0f;
            }

            return (((hoursPerDay - 1) - hour) * 3600 + (60 - minutes) * 60 + (60 - seconds)) / timeSpeed;
        }
    }

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

            if(TEST_Quest)
            {
                AddQuest(TEST_Quest);
            }
        }
    }

    void Update() 
    {
        if (gameRunning)
        {
            AdvanceTime(0, 0, Time.deltaTime * timeSpeed);
        }

        if(wantsEndOfDay && CanEndDay())
        {
            EndDay();
        }
    }

    private bool CanEndDay()
    {
        return gameRunning && LevelInstance.Instance.Mode == Mode.None;
    }
    
    private void EndDay()
    {
        LevelInstance.Instance.OpenEndDayPopup();
        wantsEndOfDay = false;
    }

    public void AdvanceTime(int hours, int minutes = 0, float seconds = 0)
    {
        if(wantsEndOfDay)
        {
            // Don't need to advance time if we are already waiting to close dialog to end the day.
            return;
        }

        this.seconds += seconds;
        if(this.seconds >= 60)
        {
            this.minutes += (int)(this.seconds / 60);
            this.seconds %= 60;
        }

        this.minutes += minutes;
        if(this.minutes >= 60)
        {
            this.hour += (int)(this.minutes / 60);
            this.minutes %= 60;
        }

        hour += hours;
        if(hour >= hoursPerDay)
        {
            if (CanEndDay())
            {
                EndDay();
            }
            else
            {
                wantsEndOfDay = true;
            }
        }

        onTimeChanged?.Invoke(hour, this.minutes);
    }

    private void Initialize()
    {
        SetMorningTime();

        Journey journey = new Journey();
        journey.destination = LevelInstance.Instance.LocationName;
        ///@todo
        journeys.Add(journey);

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

    public void DiscoverRoute(string from, string to, TransportationMethod method)
    {
        if (RouteManager.DiscoverRoute(from, to, method))
        {
            OnRouteDiscovered?.Invoke(from, to, method);
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

    private void SetDate(DateTime newDate)
    {
        DateTimeFormatInfo dateFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        date = newDate;
        //dateStr = date.ToString("dd'st', MMMM, yyyy"); //);
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
        StartNewDay();
    }

    public void OnSleepInShip(List<EndOfDayHealthData> endOfDayHealthData)
    {
        HealthStatus.OnEndOfDay(endOfDayHealthData);
        StartNewDay();

        if(ShipManager.HasReachedDestination)
        {
            // Ship travel finished, arrived in Elis island.
            SceneManager.LoadScene("LoadingScene");
        }
    }
    
    public void StartNewDay() 
    {
        day++;
        ++DaysInCity;
        SetDate(date.AddDays(1));
        SetMorningTime();
        Vibrate();
        onNewDay?.Invoke();
    }

    public void SetMorningTime() 
    {
        seconds = 0;
        hour = 0;
        minutes = 0;
        onTimeChanged?.Invoke(hour, minutes);
    }

    public static void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    public void GoToLocation(string name, TransportationMethod method)
    {
        if(nextLocation != null)
        {
            // Already travelling
            return;
        }

        if(LocationManager.IsFromEuropeToAmerica(LevelInstance.Instance.LocationName, name))
        {
            ShipManager.StartTravellingInShip();
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
            ///@todo Advance days
        }

        // Reset values
        DaysInCity = 0;
        onNewDay?.Invoke();
        SetPaused(true);

        // Load level
        nextLocation = name;
        nextMethod = method;
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
    public void ReturnToShip()
    {
        if(!ShipManager.IsStopoverDay)
        {
            return;
        }

        ShipManager.HasVisitedStopover = true;
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
        journeys.Add(journey);

        nextLocation = null;
        nextMethod = TransportationMethod.None;
        SetMorningTime();
        SetPaused(false);
    }

    public void OnLoadedShip()
    {
        if(RemainingTime > 0)
        {
            SetPaused(false);
        } 
        else
        {
            // A hack, since CanEndDay() checks whether the game is running
            StartCoroutine(EndDayNextFrame());
        }
    }

    private IEnumerator EndDayNextFrame()
    {
        yield return null;
        LevelInstance.Instance.OpenEndDayPopup();
    }

    public void OnLoadedStopover()
    {
        // Add the journey
        Journey journey = new Journey();
        journey.destination = ShipManager.StopoverLocation;
        journey.method = TransportationMethod.Ship;
        journeys.Add(journey);

        SetMorningTime();
        SetPaused(false);
    }

    public void OnLoadedElisIsland()
    {
        // Add the journey
        Journey journey = new Journey();
        journey.destination = "ElisIsland";
        journey.method = TransportationMethod.Ship;
        journeys.Add(journey);

        nextLocation = null;
        nextMethod = TransportationMethod.None;
        SetPaused(false);
        DaysInCity = 0;
        ShipManager.EndTravellingInShip();
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
        
        OnPauseChanged?.Invoke(!gameRunning);
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
        Debug.Log($"{protagonist.name} died");
        ///@todo
    }

    public void SetCurrency(Currency currency)
    {
        if(currency != CurrentCurrency)
        {
            CurrentCurrency = currency;
            onCurrencyChanged?.Invoke(CurrentCurrency);
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
