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
    //public static I//NavigationMarker //NavigationMarker;
    public static ITransportationManager TransportationManager;
    public static ICityMarker CityManager;
    //public static I//GlobeDesigner //GlobeDesigner;

    public static WorldMapGlobe map;
    private static StateManager stateManager;

    public Button CenterButton;
    public Button DiscoverLegButton;
    public Button CapitalsButton;
    public Button AnimateButton;
    public Button HideButton;
    public Text CurrentCityText;

    public Button CurrentCityButton;

    public Canvas UICanvas;

    private Country highlightedCountry;
    private Country previousHighlightedCountry;

    private City origin;
    private City destination;

    private static bool isInitialized = false;
    private static GameObject instance;

    public Text NewRoute;
    public static GameManager currentGameManager;

    public GameObject EndGamePopup;

    public GameManager()
    {
        if (stateManager == null)
        {
            stateManager = new StateManager();
        }

        currentGameManager = this;
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
            //GlobeDesigner.UpdateSelectionTexture(highlightedCountry.name, true);

            if (previousHighlightedCountry != null)
            {
                //GlobeDesigner.UpdateSelectionTexture(previousHighlightedCountry.name);
            }

            previousHighlightedCountry = highlightedCountry;
        }
        else if (highlightedCountry == null && previousHighlightedCountry != null)
        {
            //GlobeDesigner.UpdateSelectionTexture(previousHighlightedCountry.name);

            previousHighlightedCountry = null;
        }
    }

    private void Initialize()
    {
        map = WorldMapGlobe.instance;

        CityManager = new CityMarker(map);
        //NavigationMarker = new //NavigationMarker(map);
        TransportationManager = new TransportationManagerOld();
        //GlobeDesigner = new //GlobeDesigner(map);

        StateManager.CurrentState.FreezeTime = true;
        CityManager.DrawLabel(CityData.Pfaffenthal);

        //GlobeDesigner.AssignTextures();

        InitializeMissingCities();
        SetCurrentCityText();
        Subscribe();
        map.DrawCity(CityData.Pfaffenthal);
        Navigate();

        isInitialized = true;
    }

    private void InitializeMissingCities()
    {
        foreach (var city in CityData.LatLonByCity.Keys)
        {
            if (!map.cities.Any(c => c.name == city))
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
        AnimateButton.onClick.AddListener(ShowNextTransportationIllustration);
        HideButton.onClick.AddListener(DestroyTransportationIllustration);
        CurrentCityButton.onClick.AddListener(GoToCurrentCity);
        map.OnCityClick += OnCityClicked;

        //NavigationMarker.TravelCompleted += OnTravelCompleted;
        //NavigationMarker.DiscoverStarted += OnDiscoverStarted;
        //NavigationMarker.DiscoverCompleted += OnDiscoverCompleted;

        AddCustomTransportationButton.onClick.AddListener(OpenCustomTransportationForm);
        HideCustomTransportationButton.onClick.AddListener(HideCustomTransportationForm);

        OpenDiscoveryPanelButton.onClick.AddListener(OpenDiscoveryPanel);
        CloseDiscoveryPanelButton.onClick.AddListener(CloseDiscoveryPanel);
    }

    private void GoToCurrentCity()
    {
        if (StateManager.CurrentState.CurrentCityName == CityData.Pfaffenthal)
        {
            LevelManager.StartLevel("Pfaffenthal");
        }
        else if (StateManager.CurrentState.CurrentCityName == CityData.Luxembourg)
        {
            LevelManager.StartLevel("Luxembourg");
        }
    }

    private void DrawCapitals()
    {
        CityManager.DrawLabels();
    }

    private void DiscoverLeg()
    {
        foreach (var keyValue in LegData.CoordinatesByLegKey)
        {
            var leg = LegData.Legs.FirstOrDefault(l => l.Key == keyValue.Key);

            if (leg != null)
            {
                NewRoute.GetComponent<Text>().enabled = true;

                //NavigationMarker.DiscoverLeg(keyValue.Key, keyValue.Value);

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

        if (leg != null)
        {
            //NavigationMarker.DiscoverLeg(key, LegData.CoordinatesByLegKey[key]);

            CityManager.DrawLabel(leg.Origin);
            CityManager.DrawLabel(leg.Destination);

            map.DrawCity(leg.Origin);
            map.DrawCity(leg.Destination);
        }
    }

    private void TravelLeg(City origin, City destination, TransportationType type)
    {
        if (destination.name == CityData.Havre || destination.name == CityData.Rotterdam)
        {
            EndGamePopup.SetActive(true);
        }

        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            /*if (//NavigationMarker.IsLegMarked(legKey))
            {*/
                StateManager.CurrentState.PreviousCityName = origin.name;
                StateManager.CurrentState.CurrentCityName = destination.name;
                StateManager.CurrentState.AvailableMoney -= TransportationData.TransportationCostByType[type];

                map.DrawCities();

                DisplayTransportationIllustration(type);

                //NavigationMarker.TravelLeg(legKey, LegData.CoordinatesByLegKey[legKey], type);

                SetCurrentCityText();
            //}
        }
    }

    private void TravelCustomLeg(City origin, City destination, CustomTransportation transportation)
    {
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
           /* if (//NavigationMarker.IsLegMarked(legKey))
            {*/
                StateManager.CurrentState.PreviousCityName = origin.name;
                StateManager.CurrentState.CurrentCityName = destination.name;
                map.DrawCities();

                //NavigationMarker.TravelCustomLeg(legKey, LegData.CoordinatesByLegKey[legKey], transportation);

                SetCurrentCityText();
         //   }
        }
    }

    private void OnCityClicked(int cityIndex)
    {
        StateManager.CurrentState.FreezeTime = true;

        origin = map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName);
        destination = map.GetCity(cityIndex);

        ConstructTransportationOptions();
    }

    private void OnTravelCompleted()
    {
        DestroyTransportationIllustration();

        if (StateManager.CurrentState.CurrentCityName == CityData.Luxembourg ||
           StateManager.CurrentState.CurrentCityName == CityData.Paris ||
           StateManager.CurrentState.CurrentCityName == CityData.Brussels)
        {
            LevelManager.StartLevel(StateManager.CurrentState.CurrentCityName);
        }
    }

    private void OnDiscoverStarted()
    {
        NewRoute.GetComponent<Text>().enabled = true;
    }

    private void OnDiscoverCompleted()
    {
        NewRoute.GetComponent<Text>().enabled = false;
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
            map.ZoomTo(0f, 4f).Then(() => LevelManager.StartLevel("Pfaffenthal"));
        });
    }

    private void SetCurrentCityText()
    {
        CurrentCityText.GetComponent<Text>().text = $"{StateManager.CurrentState.CurrentCityName}";
    }

    private void NavigateCurrentCity()
    {
        if (StateManager.CurrentState == null)
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
