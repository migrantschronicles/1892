using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WPM;

public class GameManager : MonoBehaviour
{
    private INavigationMarker NavigationMarker;
    private ITransportationManager TransportationManager;
    private ICityMarker CityManager;
    private IGlobeDesigner GlobeDesigner;

    private WorldMapGlobe map;
    private State state;

    public Button CenterButton;
    public Button DiscoverLegButton;
    public Button CapitalsButton;
    public Text CurrentCityText;

    public Canvas UICanvas;
    public GameObject TransportationHolder;
    public List<RawImage> Transportation;

    public GameObject TransportationButtonHolder;
    public List<Button> TransportationButton;

    public ScrollRect TransportationPopup; 

    private Country highlightedCountry;
    private Country previousHighlightedCountry;

    private City origin;
    private City destination;

    public GameManager()
    {
        state = new State()
        {
            CurrentCityName = CityData.Luxembourg,
            AvailableCityNames = new List<string>() { CityData.Luxembourg },
            VisitedCityNames = new List<string>() { CityData.Luxembourg }
        };
    }

    void Start()
    {
        Initialize();
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

        //CityManager.DrawLabels();
        CityManager.DrawLabel(CityData.Luxembourg);

        GlobeDesigner.AssignTextures();

        InitializeMissingCities();
        Navigate();
        SetCurrentCityText();
        Subscribe();
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
        map.OnCityClick += OnCityClicked;

        NavigationMarker.TravelCompleted += OnTravelCompleted;
        NavigationMarker.DiscoverCompleted += OnDiscoverCompleted;
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

    private void OnCityClicked(int cityIndex)
    {
        origin = map.GetCity(CityData.CountryByCity[state.CurrentCityName], state.CurrentCityName);
        destination = map.GetCity(cityIndex);
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                TransportationPopup.transform.SetParent(UICanvas.transform, true);
            }
        }
    }

    public void OnTransportationClicked(string transportation)
    {
        if(!string.IsNullOrEmpty(transportation))
        {
            if(Enum.TryParse(transportation, out TransportationType type))
            {
                TravelLeg(origin, destination, type);

                TransportationPopup.transform.SetParent(null);
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
                state.PreviousCityName = origin.name;
                state.CurrentCityName = destination.name;

                var transportationUI = Transportation.FirstOrDefault(t => t.name == type.ToString());

                if(transportationUI != null)
                {
                    transportationUI.transform.SetParent(UICanvas.transform, true);
                }

                NavigationMarker.TravelLeg(legKey, LegData.CoordinatesByLegKey[legKey]);

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
        map.SetZoomLevel(0.05f);
        map.FlyToLocation(map.GetCity(CityData.CountryByCity[state.CurrentCityName], state.CurrentCityName).latlon);
    }

    private void SetCurrentCityText()
    {
        CurrentCityText.GetComponent<Text>().text = $"Current City: {state.CurrentCityName}";
    }

    private void NavigateCurrentCity()
    {
        var initSpeed = map.navigationTime;

        map.navigationTime = 1;
        var luxIndex = map.GetCountryIndex("Luxembourg");
        map.FlyToLocation(map.GetCity(CityData.CountryByCity[state.CurrentCityName], state.CurrentCityName).latlon);
        map.ZoomTo(0, 1);

        map.navigationTime = initSpeed;
    }
}
