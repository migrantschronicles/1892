using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WPM;

public class DiscoverLegBehaviour : MonoBehaviour
{
    public InputField Lat1;
    public InputField Lon1;

    public InputField Lat2;
    public InputField Lon2;

    public InputField Lat3;
    public InputField Lon3;

    public InputField Duration;

    public Dropdown Origin;
    public Dropdown Destination;

    public Button DiscoverButton;

    void Start()
    {
        DiscoverButton.onClick.AddListener(DiscoverLeg);

        Origin.options = GameManager.map.cities.Where(c => c.cityClass == CITY_CLASS.COUNTRY_CAPITAL).Select(c => new Dropdown.OptionData(c.name)).OrderBy(c => c.text).ToList();
        Destination.options = GameManager.map.cities.Where(c => c.cityClass == CITY_CLASS.COUNTRY_CAPITAL).Select(c => new Dropdown.OptionData(c.name)).OrderBy(c => c.text).ToList();
    }

    private void DiscoverLeg()
    {
        double lat1 = 0, lon1 = 0;
        double lat2 = 0, lon2 = 0;
        double lat3 = 0, lon3 = 0;

        bool isLatLon1 = !string.IsNullOrWhiteSpace(Lat1.text) && !string.IsNullOrWhiteSpace(Lon1.text);
        bool isLatLon2 = !string.IsNullOrWhiteSpace(Lat2.text) && !string.IsNullOrWhiteSpace(Lon2.text);
        bool isLatLon3 = !string.IsNullOrWhiteSpace(Lat3.text) && !string.IsNullOrWhiteSpace(Lon3.text);

        if (Origin.value == Destination.value ||
           isLatLon1 && (!double.TryParse(Lat1.text, out lat1) || !double.TryParse(Lon1.text, out lon1)) ||
           isLatLon2 && (!double.TryParse(Lat2.text, out lat2) || !double.TryParse(Lon2.text, out lon2)) ||
           isLatLon3 && (!double.TryParse(Lat3.text, out lat3) || !double.TryParse(Lon3.text, out lon3)))
        {
            return;
        }

        var coordinates = new List<Vector2>();

        coordinates.Add(GameManager.map.cities.First(c => c.name == Origin.options[Origin.value].text).latlon);

        if (isLatLon1)
        {
            coordinates.Add(new Vector2((float)lat1, (float)lon1));
        }

        if (isLatLon2)
        {
            coordinates.Add(new Vector2((float)lat2, (float)lon2));
        }

        if (isLatLon3)
        {
            coordinates.Add(new Vector2((float)lat3, (float)lon3));
        }

        coordinates.Add(GameManager.map.cities.First(c => c.name == Destination.options[Destination.value].text).latlon);

        var duration = 5;

        if(int.TryParse(Duration.text, out int intValue))
        {
            duration = intValue;
        }

        GameManager.NavigationMarker.DiscoverLegCustom(coordinates, duration);

        GameManager.CityManager.DrawLabel(Origin.options[Origin.value].text);
        GameManager.CityManager.DrawLabel(Destination.options[Destination.value].text);

        GameManager.map.DrawCity(Origin.options[Origin.value].text);
        GameManager.map.DrawCity(Destination.options[Destination.value].text);

        Destroy(gameObject);
    }
}
