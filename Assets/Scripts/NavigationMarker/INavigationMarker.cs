using System;
using System.Collections.Generic;
using UnityEngine;

public interface INavigationMarker
{
    Action TravelCompleted { get; set; }

    Action DiscoverCompleted { get; set; }

    void DiscoverLeg(string legKey, IEnumerable<Vector2> coordinates);

    void TravelLeg(string legKey, IEnumerable<Vector2> coordinates, TransportationType transportation);

    void TravelCustomLeg(string legKey, IEnumerable<Vector2> coordinates, CustomTransportation transportation);

    bool IsLegMarked(string legKey);

    bool IsLegTraveled(string legKey);

    void ClearLeg(string legKey);

    double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude);

    double GetDistance(Vector2 start, Vector2 end);
}
