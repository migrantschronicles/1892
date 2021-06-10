using System.Collections.Generic;
using UnityEngine;
using WPM;

public interface INavigationMarker
{
    LineMarkerAnimator MarkLeg(IEnumerable<Vector2> coordinates);

    LineMarkerAnimator TravelLeg(IEnumerable<Vector2> coordinates, float duration);

    void ClearLeg(LineMarkerAnimator leg);
}
