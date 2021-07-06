using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private StateManager stateManager;

    private int currentTransportationComponent = 0;

    public Button CenterButton;
    public Button DiscoverLegButton;
    public Button CapitalsButton;
    public Button AnimateButton;
    public Button HideButton;
    public Text CurrentCityText;

    public Canvas UICanvas;
    public GameObject TransportationHolder;
    public List<Component> Transportation;

    public GameObject TransportationButtonHolder;
    public List<Button> TransportationButton;

    public ScrollRect TransportationPopup; 

    private Country highlightedCountry;
    private Country previousHighlightedCountry;

    private City origin;
    private City destination;

    public GameManager()
    {
        stateManager = new StateManager();
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

        StateManager.CurrentState.FreezeTime = true;
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
        AnimateButton.onClick.AddListener(ShowNextTransportation);
        HideButton.onClick.AddListener(HideTransportation);
        map.OnCityClick += OnCityClicked;

        NavigationMarker.TravelCompleted += OnTravelCompleted;
        NavigationMarker.DiscoverCompleted += OnDiscoverCompleted;
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

    private void OnCityClicked(int cityIndex)
    {
        StateManager.CurrentState.FreezeTime = true;

        origin = map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName);
        destination = map.GetCity(cityIndex);
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey))
        {
            if (NavigationMarker.IsLegMarked(legKey))
            {
                TransportationPopup.transform.SetParent(UICanvas.transform, true);

                foreach(var button in TransportationButton)
                {
                    if(Enum.TryParse(button.name, out TransportationType type))
                    {
                        var cost = button.transform.GetComponentsInChildren<Text>().FirstOrDefault(t => t.name == "Cost");
                        var luggageSpace = button.transform.GetComponentsInChildren<Text>().FirstOrDefault(t => t.name == "LuggageSpace");
                        var duration = button.transform.GetComponentsInChildren<Text>().FirstOrDefault(t => t.name == "Duration");

                        if (cost != null)
                        {
                            cost.text = $"Cost: {TransportationData.TransportationCostByType[type]}";
                        }

                        if (luggageSpace != null)
                        {
                            luggageSpace.text = $"Luggage: {TransportationData.TransportationSpaceByType[type]}";
                        }

                        if (duration != null)
                        {
                            var distance = NavigationMarker.GetDistance(LegData.CoordinatesByLegKey[legKey].First(), LegData.CoordinatesByLegKey[legKey].Last());
                            var time = TimeSpan.FromHours(distance / 1000 / TransportationData.TransportationSpeedByType[type]).ToString(@"dd\:hh\:mm");
                            duration.text = $"Duration: {time}";
                        }
                    }
                }
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

                StateManager.CurrentState.AvailableMoney -= TransportationData.TransportationCostByType[type];
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
            map.ZoomTo(0f, 4f);
        });
    }

    private void SetCurrentCityText()
    {
        CurrentCityText.GetComponent<Text>().text = $"{StateManager.CurrentState.CurrentCityName}";
    }

    private void NavigateCurrentCity()
    {
        var initSpeed = map.navigationTime;

        map.navigationTime = 1;
        var luxIndex = map.GetCountryIndex("Luxembourg");
        map.FlyToLocation(map.GetCity(CityData.CountryByCity[StateManager.CurrentState.CurrentCityName], StateManager.CurrentState.CurrentCityName).latlon);
        map.ZoomTo(0, 1);

        map.navigationTime = initSpeed;
    }
}
