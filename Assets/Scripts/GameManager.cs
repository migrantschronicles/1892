using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPM;

public class GameManager : MonoBehaviour
{
    private INavigationMarker NavigationMarker;
    private ITransportationManager TransportationManager;
    private ICityManager CityManager;

    private WorldMapGlobe map;
    private State state;

    public Button CenterButton;
    public Text CurrentCity;

    public GameManager()
    {
        state = new State()
        {
            CurrentCityName = CityData.Luxembourg,
            AvailableCityNames = CityData.CityNames,
            VisitedCityNames = new List<string>() { CityData.Luxembourg }
        };
    }

    void Start()
    {
        Initialize();

        CityManager.DrawLabels();
    }

    void Update()
    {
        
    }

    private void Initialize()
    {
        map = WorldMapGlobe.instance;

        CityManager = new CityManager(map);
        NavigationMarker = new NavigationMarker(map);
        TransportationManager = new TransportationManager();

        SetCurrentCityText();
        Subscribe();
    }

    private void Subscribe()
    {
        CenterButton.onClick.AddListener(NavigateCurrentCity);
        map.OnCityClick += OnCityClicked;
    }

    private void OnCityClicked(int cityIndex)
    {
        var currentCity = map.GetCity(CityData.CountryByCity[state.CurrentCityName], state.CurrentCityName);
        var nextCity = map.GetCity(cityIndex);

        if(currentCity != null && nextCity != null && CityData.CoordinatesByCity.ContainsKey((currentCity.name, nextCity.name)))
        {
            state.PreviousCityName = currentCity.name;
            state.CurrentCityName = nextCity.name;

            NavigationMarker.MarkLeg(CityData.CoordinatesByCity[(state.PreviousCityName, state.CurrentCityName)]);
            NavigationMarker.TravelLeg(CityData.CoordinatesByCity[(state.PreviousCityName, state.CurrentCityName)], 10);

            SetCurrentCityText();
        }
    }

    private void SetCurrentCityText()
    {
        CurrentCity.GetComponent<Text>().text = $"Current City: {state.CurrentCityName}";
    }

    private void NavigateCurrentCity()
    {
        throw new NotImplementedException();
    }
}
