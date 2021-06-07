using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class NavigationMarker : INavigationMarker
{
    private WorldMapGlobe map;

    public NavigationMarker(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public IEnumerable<LineMarkerAnimator> MarkPath(IEnumerable<Vector2> coordinates, Color color)
    {
        var coordinateList = coordinates.ToList();
        var path = new List<LineMarkerAnimator>(coordinateList.Count - 1);

        for (int i = 1; i < coordinateList.Count; i++)
        {
            var line = map.AddLine(coordinateList[i - 1], coordinateList[i], color, 0, 0, 0.001f, 0);
            path.Add(line);
        }

        return path;
    }

    public IEnumerable<LineMarkerAnimator> TravelPath(IEnumerable<Vector2> coordinates, Color color, float speed)
    {
        var coordinateList = coordinates.ToList();
        var path = new List<LineMarkerAnimator>(coordinateList.Count - 1);

        for (int i = 1; i < coordinateList.Count; i++)
        {
            var line = map.AddLine(coordinateList[i - 1], coordinateList[i], color, 0, speed, 0.001f, 0);

            line.OnLineDrawingEnd += (LineMarkerAnimator args) =>
            {

            };

            path.Add(line);
        }

        return path;
    }

    private void TravelLine(LineMarkerAnimator args)
    {

    }

    public void ClearPath(IEnumerable<LineMarkerAnimator> path)
    {
        foreach(var line in path)
        {
            map.ClearLineMarker(line);
        }
    }
}

public class LineMarkerEventArgs : LineMarkerAnimator
{
    public Vector2 CurrentLocation { get; set; }

    public Vector2 NextLocation { get; set; }
}
