using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class NavigationMarker : INavigationMarker
{
    private const float MarkLineWidth = 0.0006f;
    private const float TravelLineWidth = 0.0004f;

    private static Color MarkLineColor = new Color(112f / 255f, 112f / 255f, 110f / 255f, 0.4f);
    private static Color TravelLineColor = new Color(255f / 255f, 234f / 255f, 0f / 255f);

    private readonly WorldMapGlobe map;

    private Dictionary<string, LineMarkerAnimator> markedLegMarkers = new Dictionary<string, LineMarkerAnimator>();
    private Dictionary<string, LineMarkerAnimator> traveledLegMarkers = new Dictionary<string, LineMarkerAnimator>();

    private bool isNavigating;

    public Action TravelCompleted { get; set; }

    public Action DiscoverStarted { get; set; }

    public Action DiscoverCompleted { get; set; }

    public NavigationMarker(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public void DiscoverLeg(string legKey, IEnumerable<Vector2> coordinates)
    {
        if (isNavigating) return;

        isNavigating = true;

        DiscoverStarted?.Invoke();

        var marker = map.AddLineCustom(coordinates.ToArray(), MarkLineColor, MarkLineWidth, 3);
        marker.OnLineDrawingEnd += (e) => DiscoverCompleted?.Invoke();

        if (!markedLegMarkers.ContainsKey(legKey))
        {
            markedLegMarkers.Add(legKey, marker);
        }

        Navigate(coordinates.First(), coordinates.Last(), 3, 35);
    }

    public void DiscoverLegCustom(IEnumerable<Vector2> coordinates, int duration)
    {
        if (isNavigating) return;

        isNavigating = true;

        DiscoverStarted?.Invoke();

        var marker = map.AddLineCustom(coordinates.ToArray(), MarkLineColor, MarkLineWidth, duration);
        marker.OnLineDrawingEnd += (e) => DiscoverCompleted?.Invoke();

        Navigate(coordinates.First(), coordinates.Last(), duration, 35);
    }

    public void TravelCustomLeg(string legKey, IEnumerable<Vector2> coordinates, CustomTransportation transportation)
    {
        if (isNavigating) return;

        isNavigating = true;

        var distance = GetDistance(coordinates.First().x, coordinates.First().y, coordinates.Last().x, coordinates.Last().y);
        var duration = Math.Max((float)(distance / (transportation.Speed * 1000)), 1f);
        duration = Math.Max(duration, 5);

        var marker = map.AddLineCustom(coordinates.ToArray(), TravelLineColor, TravelLineWidth, duration);
        marker.OnLineDrawingEnd += (e) => TravelCompleted?.Invoke();

        if (!traveledLegMarkers.ContainsKey(legKey))
        {
            traveledLegMarkers.Add(legKey, marker);
        }

        Navigate(coordinates.First() + new Vector2(1f, 0), coordinates.Last() + new Vector2(1f, 0), duration, 50, transportation);
    }

    public void TravelLeg(string legKey, IEnumerable<Vector2> coordinates, TransportationType transportation)
    {
        if (isNavigating) return;

        isNavigating = true;

        var distance = GetDistance(coordinates.First().x, coordinates.First().y, coordinates.Last().x, coordinates.Last().y);
        var duration = Math.Max((float)(distance / (TransportationData.TransportationSpeedByType[transportation] * 1000)), 1f);
        duration = Math.Max(duration, 5);

        var marker = map.AddLineCustom(coordinates.ToArray(), TravelLineColor, TravelLineWidth, duration);
        marker.OnLineDrawingEnd += (e) => TravelCompleted?.Invoke();

        if (!traveledLegMarkers.ContainsKey(legKey))
        {
            traveledLegMarkers.Add(legKey, marker);
        }

        Navigate(coordinates.First() + new Vector2(1f, 0), coordinates.Last() + new Vector2(1f, 0), duration, 50, transportation);
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

    private void Navigate(Vector2 start, Vector2 end, float duration, float pitch = 35)
    {
        var initZoom = map.GetZoomLevel();
        var initPitch = map.pitch;

        map.pitch = pitch;
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

    private void Navigate(Vector2 start, Vector2 end, float duration, float pitch, TransportationType transportation)
    {
        var initZoom = map.GetZoomLevel();
        var initPitch = map.pitch;

        map.pitch = pitch;
        map.SetZoomLevel(0);
        map.FlyToLocation(start - new Vector2(1f, 0), 0).Then(() =>
        {
            map.FlyToLocation(end - new Vector2(1f, 0), duration, 0.02f).Then(() =>
            {
                map.FlyToLocation(start - new Vector2(1f, 0), 0, initZoom);
                map.pitch = initPitch;
                isNavigating = false;
                StateManager.CurrentState.FreezeTime = false;
                StateManager.CurrentState.ElapsedTime += TimeSpan.FromHours(GetDistance(start, end) / 1000 / TransportationData.TransportationSpeedByType[transportation]);
            });
        });
    }

    private void Navigate(Vector2 start, Vector2 end, float duration, float pitch, CustomTransportation transportation)
    {
        var initZoom = map.GetZoomLevel();
        var initPitch = map.pitch;

        map.pitch = pitch;
        map.SetZoomLevel(0);
        map.FlyToLocation(start, 0).Then(() =>
        {
            map.FlyToLocation(end, duration, 0.03f).Then(() =>
            {
                map.FlyToLocation(start, 0, initZoom);
                map.pitch = initPitch;
                isNavigating = false;
                StateManager.CurrentState.FreezeTime = false;
                StateManager.CurrentState.ElapsedTime += TimeSpan.FromHours(GetDistance(start, end) / 1000 / transportation.Speed);
            });
        });
    }

    public double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
    {
        var d1 = latitude * (Math.PI / 180.0);
        var num1 = longitude * (Math.PI / 180.0);
        var d2 = otherLatitude * (Math.PI / 180.0);
        var num2 = otherLongitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

    public double GetDistance(Vector2 start, Vector2 end)
    {
        var d1 = start.x * (Math.PI / 180.0);
        var num1 = start.y * (Math.PI / 180.0);
        var d2 = end.x * (Math.PI / 180.0);
        var num2 = end.y * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}
