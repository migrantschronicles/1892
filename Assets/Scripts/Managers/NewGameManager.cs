using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using System;

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

public class NewGameManager : MonoBehaviour
{

    public string userName;
    
    public string currentLocation;
    public List<string> visitedLocationsStr;

    private static bool isInitialized = false;
    
    // Game Stats
    public bool gameRunning = true;
    public float timeSpeed = 0.1f;
    public int hoursPerDay = 10;
    public float seconds;
    public int minutes;
    public int hour;
    public int day = 0;

    

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

    public LocationMarker CurrentLocationObject
    {
        get
        {
            return LevelInstance.Instance.IngameDiary.LocationMarkerObjects.First(marker => marker.LocationName == currentLocation);
        }
    }

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
            UpdateTime();
        }
    }

    private void UpdateTime() 
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

        if (hour >= hoursPerDay)
        {
            popups.OpenEndDayPopUp();
            popups.backBTNEndDay.SetActive(false);
        }

        onTimeChanged?.Invoke(hour, minutes);
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

    private void SetDate(DateTime newDate)
    {
        DateTimeFormatInfo dateFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        date = newDate;
        dateStr = date.ToString(dateFormat.ShortDatePattern);
        onDateChanged?.Invoke(date);
    }

    public void SleepOutside(int motherFoodAmount, int boyFoodAmount, int girlFoodAmount) {
        int totalFoodUsed = motherFoodAmount + boyFoodAmount + girlFoodAmount;

        if (totalFoodUsed != 0) { 
            // Apply Health
            inventory.RemoveItemCategory(foodCategory, totalFoodUsed);
        }

        ///@todo Should be passed as a parameter
        HealthStatus.OnEndOfDay(new EndOfDayHealthData[]
        {
            new EndOfDayHealthData { name = "Elis", foodAmount = motherFoodAmount },
            new EndOfDayHealthData { name = "Mreis", foodAmount = girlFoodAmount },
            new EndOfDayHealthData { name = "Mattis", foodAmount = boyFoodAmount }
        });
    }

    public void SleepInHostel(int cost,int purchasedFoodAmount, int motherFoodAmount, int boyFoodAmount, int girlFoodAmount) {
        money -= cost;


        int totalFoodUsed = motherFoodAmount + boyFoodAmount + girlFoodAmount;
        int remainingFood = totalFoodUsed - purchasedFoodAmount;

        if (totalFoodUsed != 0) { } // Apply health

        if (totalFoodUsed - purchasedFoodAmount > 0)
        {
            inventory.RemoveItemCategory(foodCategory, totalFoodUsed-purchasedFoodAmount);
        }
        if(purchasedFoodAmount - totalFoodUsed > 0) 
        {
            inventory.AddItem(foodItem, purchasedFoodAmount - totalFoodUsed);
        }

        ///@todo Should be passed as a parameter
        HealthStatus.OnEndOfDay(new EndOfDayHealthData[]
        {
            new EndOfDayHealthData { name = "Elis", foodAmount = motherFoodAmount },
            new EndOfDayHealthData { name = "Mreis", foodAmount = girlFoodAmount },
            new EndOfDayHealthData { name = "Mattis", foodAmount = boyFoodAmount }
        });
    }
    
    public void StartNewDay() 
    {
        Debug.Log("Go to next day here, via clock");
        day++;
        ++DaysInCity;
        onNewDay?.Invoke();
        SetDate(date.AddDays(1));
        SetMorningTime();
        Vibrate();

        // Conditions for Ship movement on Map
        Vector3 startPoint = new Vector3(120, 92, 0); // To be updated accordingly
        Vector3 endPoint = new Vector3(120, 10.5f, 0); // To be updated accordingly
        Vector3 controlPoint1 = new Vector3(160, 51.25f, 0); // Should be in the middle of the route.
        //Vector3 controlPoint2 = new Vector3(140, 10.5f, 0); // If needed (For curves), uncomment this.

        int TotalDaysInShip = 0; // Needs to be updated accordingly.

        float t = (DaysInCity / TotalDaysInShip) / 13.0f; // Make a scale of "days" to a value between 0 and 1
        
        // Need to uncomment the line below to allow for movement.
        //shipIcon.localPosition = Vector3.Lerp(Vector3.Lerp(startPoint, controlPoint1, t), Vector3.Lerp(controlPoint1, endPoint, t), t);

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
        DaysInCity = 0;

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
            result += "...";
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
                    result += " ...";
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
}
