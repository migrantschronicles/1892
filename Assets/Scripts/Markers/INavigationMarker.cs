using System.Collections.Generic;
using UnityEngine;
using WPM;

public interface INavigationMarker
{
    IEnumerable<LineMarkerAnimator> MarkPath(IEnumerable<Vector2> coordinates, Color color);

    IEnumerable<LineMarkerAnimator> TravelPath(IEnumerable<Vector2> coordinates, Color color, float speed);

    void ClearPath(IEnumerable<LineMarkerAnimator> path);
}
