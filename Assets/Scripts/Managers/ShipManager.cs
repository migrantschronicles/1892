using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShipRoute
{
    public string fromLocation;
    [Tooltip("If there is a stopover in a city")]
    public string stopoverLocation;
    [Tooltip("The day of the stopover during 10-day ship travel (1 is first day)")]
    public int stopoverDay;

    public bool HasStopover { get { return !string.IsNullOrWhiteSpace(stopoverLocation); } }
}

public class ShipManager : MonoBehaviour
{
    [SerializeField]
    private ShipRoute[] shipRoutes;

    public bool IsTravellingInShip { get { return currentShipRoute != null; } }
    public string FromLocation { get; private set; }
    public bool IsStopoverDay 
    { 
        get 
        {
            return currentShipRoute != null && currentShipRoute.HasStopover && NewGameManager.Instance.DaysInCity == (currentShipRoute.stopoverDay - 1);
        }
    }
    public bool WasStopoverDay
    {
        get
        {
            return currentShipRoute != null && currentShipRoute.HasStopover && NewGameManager.Instance.DaysInCity == currentShipRoute.stopoverDay;
        }
    }

    public string StopoverLocation { get { return currentShipRoute != null ? currentShipRoute.stopoverLocation : null; } }
    public int TravelDays { get { return 10; } }
    public bool HasReachedDestination { get { return currentShipRoute != null && NewGameManager.Instance.DaysInCity >= TravelDays; } }
    public bool WantsToVisitStopover { get; set; }
    public bool HasVisitedStopover { get; set; }

    private ShipRoute currentShipRoute = null;

    public void StartTravellingInShip()
    {
        FromLocation = LevelInstance.Instance.LocationName;
        currentShipRoute = GetShipRoute(FromLocation);
    }

    public void EndTravellingInShip()
    {
        currentShipRoute = null;
    }

    private ShipRoute GetShipRoute(string fromLocation)
    {
        return shipRoutes.FirstOrDefault(route => route.fromLocation == fromLocation);
    }
}
