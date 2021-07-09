using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscoverLegBehaviour : MonoBehaviour
{
    public InputField Lat1;
    public InputField Lon1;

    public InputField Lat2;
    public InputField Lon2;

    public InputField Lat3;
    public InputField Lon3;

    public Dropdown Origin;
    public Dropdown Destination;

    public Button DiscoverButton;

    void Start()
    {
        DiscoverButton.onClick.AddListener(DiscoverLeg);

        Origin.options = new List<Dropdown.OptionData>() 
        {
            new Dropdown.OptionData(CityData.Luxembourg),
            new Dropdown.OptionData(CityData.Brussels),
            new Dropdown.OptionData(CityData.France),
            new Dropdown.OptionData(CityData.Belgium),
            new Dropdown.OptionData(CityData.Havre),
            new Dropdown.OptionData(CityData.Antwerp)
        };

        Destination.options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData(CityData.Luxembourg),
            new Dropdown.OptionData(CityData.Brussels),
            new Dropdown.OptionData(CityData.France),
            new Dropdown.OptionData(CityData.Belgium),
            new Dropdown.OptionData(CityData.Havre),
            new Dropdown.OptionData(CityData.Antwerp)
        };
    }

    private void DiscoverLeg()
    {
        var coordinates = new List<Vector2>();

        coordinates.Add(CityData.LatLonByCity[Origin.options[Origin.value].text]);

        if (double.TryParse(Lat1.text, out double lat1) && double.TryParse(Lon1.text, out double lon1))
        {
            coordinates.Add(new Vector2((float)lat1, (float)lon1));
        }

        if (double.TryParse(Lat2.text, out double lat2) && double.TryParse(Lon2.text, out double lon2))
        {
            coordinates.Add(new Vector2((float)lat2, (float)lon2));
        }

        if (double.TryParse(Lat3.text, out double lat3) && double.TryParse(Lon3.text, out double lon3))
        {
            coordinates.Add(new Vector2((float)lat3, (float)lon3));
        }

        coordinates.Add(CityData.LatLonByCity[Destination.options[Origin.value].text]);

        GameManager.NavigationMarker.DiscoverLegCustom(coordinates);

        Destroy(gameObject);
    }
}
