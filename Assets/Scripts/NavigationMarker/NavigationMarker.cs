using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class NavigationMarker : INavigationMarker
{
    private const float MarkLineWidth = 0.001f;
    private const float TravelLineWidth = 0.0005f;

    private static Color MarkLineColor = new Color(196f / 255f, 184f / 255f, 149f / 255f, 0.3f);
    private static Color TravelLineColor = new Color(240f / 255f, 210f / 255f, 122f / 255f);

    private readonly WorldMapGlobe map;

    private Dictionary<string, LineMarkerAnimator> markedLegMarkers = new Dictionary<string, LineMarkerAnimator>();
    private Dictionary<string, LineMarkerAnimator> traveledLegMarkers = new Dictionary<string, LineMarkerAnimator>();

    private bool isNavigating;

    public NavigationMarker(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public void DiscoverLeg(string legKey, IEnumerable<Vector2> coordinates)
    {
        if (isNavigating) return;

        isNavigating = true;

        var marker = map.AddLineCustom(coordinates.ToArray(), MarkLineColor, MarkLineWidth, 3);

        if (!markedLegMarkers.ContainsKey(legKey))
        {
            markedLegMarkers.Add(legKey, marker);
        }

        var distance = GetDistance(coordinates.First().x, coordinates.First().y, coordinates.Last().x, coordinates.Last().y);
        var duration = Math.Max((float)(distance/55000), 1f);

        Navigate(coordinates.First(), coordinates.Last(), duration);
    }

    public void TravelLeg(string legKey, IEnumerable<Vector2> coordinates)
    {
        if (isNavigating) return;

        isNavigating = true;

        //todo: calculate relative duration
        //var transportationSpeed = TransportationData.TransportationByCity[(origin, destination)].FirstOrDefault(t => t.Type == type).;
        //var duration = TransportationData.TransportationSpeedByType[type];

        var marker = map.AddLineCustom(coordinates.ToArray(), TravelLineColor, TravelLineWidth, 10);

        if (!traveledLegMarkers.ContainsKey(legKey))
        {
            traveledLegMarkers.Add(legKey, marker);
        }

        var distance = GetDistance(coordinates.First().x, coordinates.First().y, coordinates.Last().x, coordinates.Last().y);
        var duration = Math.Max((float)(distance / 20000), 1f);

        Navigate(coordinates.First(), coordinates.Last(), 10);
    }

    public bool IsLegMarked(string legKey)
    {
        return markedLegMarkers.ContainsKey(legKey);
    }

    public bool IsLegTraveled(string legKey)
    {
        return traveledLegMarkers.ContainsKey(legKey);
    }

    public void ClearLeg(string legKey)
    {
        if (markedLegMarkers.ContainsKey(legKey))
        {
            map.ClearLineMarker(markedLegMarkers[legKey]);
            markedLegMarkers.Remove(legKey);
        }

        if (traveledLegMarkers.ContainsKey(legKey))
        {
            map.ClearLineMarker(traveledLegMarkers[legKey]);
            traveledLegMarkers.Remove(legKey);
        }
    }

    private void Navigate(Vector2 start, Vector2 end, float duration)
    {
        var initZoom = map.GetZoomLevel();
        var initPitch = map.pitch;

        map.pitch = 35;
        map.SetZoomLevel(0);
        map.FlyToLocation(start, 0).Then(() => 
        {
            map.FlyToLocation(end, duration, 0.03f).Then(() =>
            {
                map.FlyToLocation(start, 0, initZoom);
                map.pitch = initPitch;
                isNavigating = false;
            });
        });
    }

    private double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
    {
        var d1 = latitude * (Math.PI / 180.0);
        var num1 = longitude * (Math.PI / 180.0);
        var d2 = otherLatitude * (Math.PI / 180.0);
        var num2 = otherLongitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}
