using System;
using System.Collections.Generic;
using UnityEngine;

public interface INavigationMarker
{
    Action TravelCompleted { get; set; }

    Action DiscoverCompleted { get; set; }

    void DiscoverLeg(string legKey, IEnumerable<Vector2> coordinates);

    void TravelLeg(string legKey, IEnumerable<Vector2> coordinates);

    bool IsLegMarked(string legKey);

    bool IsLegTraveled(string legKey);

    void ClearLeg(string legKey);
}
