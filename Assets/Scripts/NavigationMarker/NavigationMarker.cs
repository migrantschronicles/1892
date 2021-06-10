using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class NavigationMarker : INavigationMarker
{
    private const float MarkLineWidth = 0.001f;
    private const float TravelLineWidth = 0.0007f;

    private static Color MarkLineColor = new Color(196f / 255f, 184f / 255f, 149f / 255f, 0.3f);
    private static Color TravelLineColor = new Color(240f / 255f, 210f / 255f, 122f / 255f);

    private readonly WorldMapGlobe map;

    public NavigationMarker(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public LineMarkerAnimator MarkLeg(IEnumerable<Vector2> coordinates)
    {
        return map.AddLineCustom(coordinates.ToArray(), MarkLineColor, MarkLineWidth);
    }

    public LineMarkerAnimator TravelLeg(IEnumerable<Vector2> coordinates, float duration)
    {
        return map.AddLineCustom(coordinates.ToArray(), TravelLineColor, TravelLineWidth, duration);
    }

    public void ClearLeg(LineMarkerAnimator leg)
    {
        map.ClearLineMarker(leg);
    }
}
