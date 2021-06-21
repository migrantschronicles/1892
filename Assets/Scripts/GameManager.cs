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

    private Country highlightedCountry;
    private Country previousHighlightedCountry;

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

                break;
            }
        }
    }

    private void OnCityClicked(int cityIndex)
    {
        var currentCity = map.GetCity(CityData.CountryByCity[state.CurrentCityName], state.CurrentCityName);
        var nextCity = map.GetCity(cityIndex);
        var legKey = currentCity.name + nextCity.name + TransportationType.Foot;

        if (currentCity != null && nextCity != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                state.PreviousCityName = currentCity.name;
                state.CurrentCityName = nextCity.name;

                NavigationMarker.TravelLeg(legKey, LegData.CoordinatesByLegKey[legKey]);

                SetCurrentCityText();
            }
        }
    }

    private void Navigate()
    {
        map.SetZoomLevel(0);
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
