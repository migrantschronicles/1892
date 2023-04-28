using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public class DiscoveredRoute
    {
        public string from;
        public string to;
        public TransportationMethod method = TransportationMethod.None;
    }

    private List<DiscoveredRoute> routes = new();

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

    public bool IsRouteDiscovered(string from, string to, TransportationMethod method = TransportationMethod.None)
    {
        foreach(var route in routes)
        {
            if(route.from == from && route.to == to && (method == TransportationMethod.None || route.method == method))
            {
                return true;
            }
        }

        return false;
    }

    /**
     * Discovers a new route.
     * @return True if a new route is discovered, false if it fails or is already discovered.
     */
    public bool DiscoverRoute(string from, string to, TransportationMethod method)
    {
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
