using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class CityManager : ICityManager
{
    private static Vector2 LabelOffset = new Vector2(-0.2f, 0.02f);
    private static Color LabelColor = new Color(210f / 255f, 260f / 255f, 202f / 255f);
    private static float LabelScale = 0.001f;

    private readonly WorldMapGlobe map;

    public CityManager(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public void DrawLabels()
    {
        foreach (var name in CityData.CityNames)
        {
            var labelPosition = CityData.CityPosition[name] + LabelOffset;

            map.AddText(name, Conversion.GetSpherePointFromLatLon(labelPosition.x, labelPosition.y), LabelColor, LabelScale);
        }

        foreach (var city in map.cities.Where(c => c.cityClass == CITY_CLASS.COUNTRY_CAPITAL))
        {
            if (!CityData.CityNames.Contains(city.name))
            {
                var labelPosition = city.latlon + LabelOffset;

                map.AddText(city.name, Conversion.GetSpherePointFromLatLon(labelPosition.x, labelPosition.y), LabelColor, LabelScale);
            }
        }
    }
}
