using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HistoryModeRoute
{
    None,
    LeHavre,
    Rotterdam,
    Antwerp
}

[System.Serializable]
public class HistoryModeRouteInfo
{
    public HistoryModeRoute route;
    public string[] locations;
}

/**
 * Stores all the routes that are discovered.
 * Some routes from one city to another with a specific transportation method are available from beginning,
 * but some routes are discovered via dialogs.
 */
public class RouteManager : MonoBehaviour
{
    public class DiscoveredRoute
    {
        public string from;
        public string to;
        public TransportationMethod method = TransportationMethod.None;
    }

    private List<DiscoveredRoute> routes = new();
    [SerializeField]
    private List<HistoryModeRouteInfo> historyRoutes = new();
    private HistoryModeRoute selectedHistoryRoute = HistoryModeRoute.None; 

    public List<DiscoveredRoute> Routes { get { return routes; } }

    private void Start()
    {
        foreach(var info in NewGameManager.Instance.transportationInfo.TransportationInfo)
        {
            if(!info.isDiscoverable)
            {
                routes.Add(new DiscoveredRoute
                {
                    from = info.FromLocation,
                    to = info.ToLocation,
                    method = info.method
                });
            }
        }
    }

    public void OnHistoryModeRouteSelected(HistoryModeRoute route)
    {
        selectedHistoryRoute = route;
    }

    public bool IsRouteDiscovered(string from, string to, TransportationMethod method = TransportationMethod.None)
    {
        if(NewGameManager.Instance.isHistoryMode && to != "Luxembourg")
        {
            if(NewGameManager.Instance.HasTraveled(from, to))
            {
                return true;
            }

            if(from != LevelInstance.Instance.LocationName)
            {
                return false;
            }

            string next = GetNextHistoryModeLocation();
            if(!string.IsNullOrEmpty(next) && next == to)
            {
                return true;
            }

            return false;
        }

        foreach(var route in routes)
        {
            if(route.from == from && route.to == to && (method == TransportationMethod.None || route.method == method))
            {
                return true;
            }
        }

        return false;
    }

    public string GetHistoryModePort()
    {
        if(selectedHistoryRoute == HistoryModeRoute.None)
        {
            return "";
        }

        HistoryModeRouteInfo info = GetHistoryModeRouteInfo(selectedHistoryRoute);
        for(int i = 0; i < info.locations.Length - 1; ++i)
        {
            if (info.locations[i + 1] == "ElisIsland")
            {
                return info.locations[i];
            }
        }

        return "";
    }

    private string GetNextHistoryModeLocation()
    {
        if(selectedHistoryRoute == HistoryModeRoute.None)
        {
            return "";
        }

        HistoryModeRouteInfo info = GetHistoryModeRouteInfo(selectedHistoryRoute);
        if(info == null)
        {
            return "";
        }

        for(int i = 0; i < info.locations.Length - 1; ++i)
        {
            if (info.locations[i] == LevelInstance.Instance.LocationName)
            {
                return info.locations[i + 1];
            }
        }

        return "";
    }

    private HistoryModeRouteInfo GetHistoryModeRouteInfo(HistoryModeRoute route)
    {
        return historyRoutes.Find(r => r.route == route);
    }

    /**
     * Discovers a new route.
     * @return True if a new route is discovered, false if it fails or is already discovered.
     */
    public bool DiscoverRoute(string from, string to, TransportationMethod method)
    {
        if(NewGameManager.Instance.isHistoryMode && to != "Luxembourg")
        {
            return false;
        }

        if(method == TransportationMethod.None || IsRouteDiscovered(from, to, method))
        {
            return false;
        }

        routes.Add(new DiscoveredRoute
        {
            from = from,
            to = to,
            method = method
        });

        return true;
    }
}
