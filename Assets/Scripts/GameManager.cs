using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WPM;

public partial class GameManager : MonoBehaviour
{
    private static INavigationMarker NavigationMarker;
    private static ITransportationManager TransportationManager;
    private static ICityMarker CityManager;
    private static IGlobeDesigner GlobeDesigner;

    private static WorldMapGlobe map;
    private static StateManager stateManager;

    private int currentTransportationComponent = 0;

    public Button CenterButton;
    public Button DiscoverLegButton;
    public Button CapitalsButton;
    public Button AnimateButton;
    public Button HideButton;
    public Text CurrentCityText;

    public Button CurrentCityButton;

    public Canvas UICanvas;
    public GameObject TransportationHolder;
    public List<Component> Transportation;

    private Country highlightedCountry;
    private Country previousHighlightedCountry;

    private City origin;
    private City destination;

    private static bool isInitialized = false;
    private static GameObject instance;

    public Button TransportationButtonPrefab;
    private List<Button> transportationButtonsToDestroy = new List<Button>();

    public GameManager()
    {
        if(stateManager == null)
        {
            stateManager = new StateManager();
        }
    }

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

    void Update()
    {
        CityManager.UpdateLabels();

        UpdateHighlightedCountry();
    }

    private void UpdateHighlightedCountry()
    {
        highlightedCountry = map.countryHighlighted;

        if (highlightedCountry != null && highlightedCountry != previousHighlightedCountry)
        {
            GlobeDesigner.UpdateSelectionTexture(highlightedCountry.name, true);

            if(previousHighlightedCountry != null)
            {
                GlobeDesigner.UpdateSelectionTexture(previousHighlightedCountry.name);
            }

            previousHighlightedCountry = highlightedCountry;
        }
        else if(highlightedCountry == null && previousHighlightedCountry != null)
        {
            GlobeDesigner.UpdateSelectionTexture(previousHighlightedCountry.name);

            previousHighlightedCountry = null;
        }
    }

    private void Initialize()
    {
        map = WorldMapGlobe.instance;

        CityManager = new CityMarker(map);
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
        Navigate();

        isInitialized = true;
    }

    private void InitializeMissingCities()
    {
        foreach(var city in CityData.LatLonByCity.Keys)
        {
            if(!map.cities.Any(c => c.name == city))
            {
                var location = Conversion.GetSpherePointFromLatLon(CityData.LatLonByCity[city]);
                map.cities.Add(new City(city, string.Empty, map.GetCountryIndex(CityData.CountryByCity[city]), 0, location, CITY_CLASS.CITY));
            }
        }
    }

    private void Subscribe()
    {
        CenterButton.onClick.AddListener(NavigateCurrentCity);
        DiscoverLegButton.onClick.AddListener(DiscoverLeg);
        CapitalsButton.onClick.AddListener(DrawCapitals);
        AnimateButton.onClick.AddListener(ShowNextTransportation);
        HideButton.onClick.AddListener(HideTransportation);
        CurrentCityButton.onClick.AddListener(GoToCurrentCity);
        map.OnCityClick += OnCityClicked;

        NavigationMarker.TravelCompleted += OnTravelCompleted;
        NavigationMarker.DiscoverCompleted += OnDiscoverCompleted;

        AddCustomTransportationButton.onClick.AddListener(OpenCustomTransportationForm);
        HideCustomTransportationButton.onClick.AddListener(HideCustomTransportationForm);
    }

    private void GoToCurrentCity()
    {
        LevelManager.StartLevel("StartingScene-Pfaffenthal");
    }

    private void ShowNextTransportation()
    {
        HideTransportation();

        var transportationUI = Transportation[currentTransportationComponent % (Transportation.Count - 1)];

        if (transportationUI != null)
        {
            transportationUI.transform.SetParent(UICanvas.transform, true);

            currentTransportationComponent++;
        }
    }

    private void HideTransportation()
    {
        foreach (var transportationUI in Transportation)
        {
            transportationUI.transform.SetParent(null);
        }
    }

    private void DrawCapitals()
    {
        CityManager.DrawLabels();
    }

    private void DiscoverLeg()
    {
        foreach(var keyValue in LegData.CoordinatesByLegKey)
        {
            var leg = LegData.Legs.FirstOrDefault(l => l.Key == keyValue.Key);

            if(leg != null && !NavigationMarker.IsLegMarked(leg.Key))
            {
                NavigationMarker.DiscoverLeg(keyValue.Key, keyValue.Value);

                CityManager.DrawLabel(leg.Origin);
                CityManager.DrawLabel(leg.Destination);

                map.DrawCity(leg.Origin);
                map.DrawCity(leg.Destination);

                break;
            }
        }
    }

    public static void DiscoverLeg(string key)
    {
        var leg = LegData.Legs.FirstOrDefault(l => l.Key == key);

        if (leg != null && !NavigationMarker.IsLegMarked(leg.Key))
        {
            NavigationMarker.DiscoverLeg(key, LegData.CoordinatesByLegKey[key]);

            CityManager.DrawLabel(leg.Origin);
            CityManager.DrawLabel(leg.Destination);

            map.DrawCity(leg.Origin);
            map.DrawCity(leg.Destination);
        }
    }

    private void OnCityClicked(int cityIndex)
    {
        StateManager.CurrentState.FreezeTime = true;

        origin = map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName);
        destination = map.GetCity(cityIndex);
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey) && TransportationData.TransportationByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                float buttonWidth = 0;
                float buttonHeight = 0;

                foreach (var type in TransportationData.TransportationByLegKey[legKey])
                {
                    var button = Instantiate(TransportationButtonPrefab, Vector3.zero, Quaternion.identity);

                    button.onClick.AddListener(delegate 
                    { 
                        foreach(var button in transportationButtonsToDestroy)
                        {
                            Destroy(button.gameObject);
                        }

                        transportationButtonsToDestroy.Clear();

                        TravelLeg(origin, destination, type);
                    });

                    if (buttonWidth == 0 || buttonHeight == 0)
                    {
                        buttonWidth = button.gameObject.GetComponent<RectTransform>().rect.width;
                        buttonHeight = button.gameObject.GetComponent<RectTransform>().rect.height;
                    }

                    var iconImage = button.transform.Find("Icon").transform.GetComponent<Image>();
                    var typeText = button.transform.Find("Type").transform.GetComponent<Text>();
                    var costText = button.transform.Find("Cost").transform.GetComponentInChildren<Text>();
                    var luggageSpaceText = button.transform.Find("LuggageSpace").transform.GetComponentInChildren<Text>();
                    var durationText = button.transform.Find("Duration").transform.GetComponent<Text>();

                    if (typeText != null)
                    {
                        iconImage.sprite = Resources.Load<Sprite>($"TransportationResources/{TransportationData.TransportationIconByType[type].Name}");
                        //iconImage.rectTransform.sizeDelta = TransportationData.TransportationIconByType[type].Size;
                    }

                    if (typeText != null)
                    {
                        typeText.text = type.ToString();
                    }    

                    if (costText != null)
                    {
                        costText.text = $"{TransportationData.TransportationCostByType[type]}";
                    }

                    if (luggageSpaceText != null)
                    {
                        luggageSpaceText.text = $"{TransportationData.TransportationSpaceByType[type]}";
                    }

                    if (durationText != null)
                    {
                        var distance = NavigationMarker.GetDistance(LegData.CoordinatesByLegKey[legKey].First(), LegData.CoordinatesByLegKey[legKey].Last());
                        var time = TimeSpan.FromHours(distance / 1000 / TransportationData.TransportationSpeedByType[type]);
                        var timeString = time.Days > 0 ? $"{time.Days}d {time.Hours}h {time.Minutes}m" : $"{time.Hours}h {time.Minutes}m";
                        durationText.text = timeString;
                    }

                    transportationButtonsToDestroy.Add(button);
                }

                var count = TransportationData.TransportationByLegKey[legKey].Count();

                if(CustomTransportationBehaviour.CustomTransportationByLegKey.ContainsKey(legKey))
                {
                    count += CustomTransportationBehaviour.CustomTransportationByLegKey[legKey].Count;

                    foreach(var custom in CustomTransportationBehaviour.CustomTransportationByLegKey[legKey])
                    {
                        var button = Instantiate(TransportationButtonPrefab, Vector3.zero, Quaternion.identity);

                        button.onClick.AddListener(delegate
                        {
                            foreach (var button in transportationButtonsToDestroy)
                            {
                                Destroy(button.gameObject);
                            }

                            transportationButtonsToDestroy.Clear();

                            TravelCustomLeg(origin, destination, custom);
                        });

                        var iconImage = button.transform.Find("Icon").transform.GetComponent<Image>();
                        var typeText = button.transform.Find("Type").transform.GetComponent<Text>();
                        var costText = button.transform.Find("Cost").transform.GetComponentInChildren<Text>();
                        var luggageSpaceText = button.transform.Find("LuggageSpace").transform.GetComponentInChildren<Text>();
                        var durationText = button.transform.Find("Duration").transform.GetComponent<Text>();

                        if (typeText != null)
                        {
                            iconImage.sprite = Resources.Load<Sprite>($"TransportationResources/{custom.IconName}");
                        }

                        if (typeText != null)
                        {
                            typeText.text = custom.Type;
                        }

                        if (costText != null)
                        {
                            costText.text = $"{custom.Cost}";
                        }

                        if (luggageSpaceText != null)
                        {
                            luggageSpaceText.text = $"{custom.Luggage}";
                        }

                        if (durationText != null)
                        {
                            var distance = NavigationMarker.GetDistance(LegData.CoordinatesByLegKey[legKey].First(), LegData.CoordinatesByLegKey[legKey].Last());
                            var time = TimeSpan.FromHours(distance / 1000 / custom.Speed);
                            var timeString = time.Days > 0 ? $"{time.Days}d {time.Hours}h {time.Minutes}m" : $"{time.Hours}h {time.Minutes}m";
                            durationText.text = timeString;
                        }

                        transportationButtonsToDestroy.Add(button);
                    }
                }

                var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;
                var canvasHeight = UICanvas.gameObject.GetComponent<RectTransform>().rect.height;
                var margin = 15;
                var index = 0;

                foreach (var button in transportationButtonsToDestroy)
                {
                    button.transform.position = new Vector3((canvasWidth - count * (buttonWidth + margin)) / 2 + index++ * (buttonWidth + margin) , (canvasHeight - buttonHeight) / 2, 0);
                    button.transform.SetParent(UICanvas.transform);
                }
            }
        }
    }

    private void TravelLeg(City origin, City destination, TransportationType type)
    {
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                StateManager.CurrentState.PreviousCityName = origin.name;
                StateManager.CurrentState.CurrentCityName = destination.name;
                map.DrawCities();

                var transportationUI = Transportation.FirstOrDefault(t => t.name == type.ToString());

                if(transportationUI != null)
                {
                    transportationUI.transform.SetParent(UICanvas.transform, true);
                }

                NavigationMarker.TravelLeg(legKey, LegData.CoordinatesByLegKey[legKey], type);

                SetCurrentCityText();
            }
        }
    }

    private void TravelCustomLeg(City origin, City destination, CustomTransportation transportation)
    {
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                StateManager.CurrentState.PreviousCityName = origin.name;
                StateManager.CurrentState.CurrentCityName = destination.name;
                map.DrawCities();

                NavigationMarker.TravelCustomLeg(legKey, LegData.CoordinatesByLegKey[legKey], transportation);

                SetCurrentCityText();
            }
        }
    }

    private void OnTravelCompleted()
    {
        foreach(var transportationUI in Transportation)
        {
            transportationUI.transform.SetParent(null);
        }
    }

    private void OnDiscoverCompleted()
    {
    }

    private void Navigate()
    {
        map.SetZoomLevel(0.65f);
        map.FlyToLocation(map.GetCity("Japan", "Tokyo").latlon, 0, 0.65f).Then(() => 
        {
            map.FlyToLocation(map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName).latlon, 2, 0.65f).Then(() => 
            {
                //map.ZoomTo(0f, 1.5f);
            });
            map.ZoomTo(0f, 4f).Then(() => LevelManager.StartLevel("StartingScene-Pfaffenthal"));
        });
    }

    private void SetCurrentCityText()
    {
        CurrentCityText.GetComponent<Text>().text = $"{StateManager.CurrentState.CurrentCityName}";
    }

    private void NavigateCurrentCity()
    {
        if(StateManager.CurrentState == null)
        {
            return;
        }

        var initSpeed = map.navigationTime;

        map.navigationTime = 1;
        var luxIndex = map.GetCountryIndex("Luxembourg");
        map.FlyToLocation(map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName).latlon);
        map.ZoomTo(0, 1);

        map.navigationTime = initSpeed;
    }

    private void NavigateCurrentCity(float navTime)
    {
        if (StateManager.CurrentState == null)
        {
            return;
        }

        var initSpeed = map.navigationTime;

        map.navigationTime = navTime;
        var luxIndex = map.GetCountryIndex("Luxembourg");
        map.FlyToLocation(map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName).latlon);
        map.ZoomTo(0, navTime);

        map.navigationTime = initSpeed;
    }
}
