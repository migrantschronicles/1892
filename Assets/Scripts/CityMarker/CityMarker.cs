using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class CityMarker : ICityMarker
{
    private static Vector3 LabelOffset = new Vector3(0, 0.002f, 0.00f);
    private static Color LabelColor = new Color(79f / 255f, 15f / 255f, 17f / 255f);

    private const float MinLabelScale = 0.0015f;
    private const float MaxLabelScale = 0.0045f;

    private readonly WorldMapGlobe map;

    private IList<TextMesh> labels = new List<TextMesh>();

    public static List<string> Labels { get; } = new List<string>();

    public CityMarker(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public void DrawLabels()
    {
        foreach (var city in map.cities.Where(c => c.cityClass == CITY_CLASS.COUNTRY_CAPITAL))
        {
            var labelPosition = Conversion.GetSpherePointFromLatLon(city.latlon.x, city.latlon.y) + LabelOffset;
            var label = map.AddTextCustom(city.name, labelPosition, LabelColor, GetCurrentScale(), fontStyle: FontStyle.Bold);

            labels.Add(label);
            Labels.Add(city.name);
        }
    }

    public void DrawLabel(string name)
    {
        if (!labels.Any(l => l.text == name))
        {
            var latLon = CityData.LatLonByCity.ContainsKey(name) ? CityData.LatLonByCity[name] : map.cities.First(c => c.name == name).latlon;
            var labelPosition = Conversion.GetSpherePointFromLatLon(latLon.x, latLon.y) + LabelOffset;
            var label = map.AddTextCustom(name, labelPosition, LabelColor, GetCurrentScale(), fontStyle: FontStyle.Bold);

            labels.Add(label);
            Labels.Add(name);
        }
    }

    public void UpdateLabels()
    {
        foreach (var label in labels)
        {
            map.UpdateTextCustom(label, GetCurrentScale());
        }
    }

    public void ClearLabels()
    {
        foreach(var label in labels)
        {
            map.ClearMarker(label.GetComponent<GameObject>());
        }

        labels.Clear();
        Labels.Clear();
    }

    private float GetCurrentScale()
    {
        return Math.Min(MaxLabelScale, MinLabelScale + map.GetZoomLevel() / 100);
    }
}
