using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GeoJSONLocation
{
    public string location;
    public double lat;
    public double lon;
}

/**
 * Stores the geojson location of each city.
 * Calculates a list of coordinate for a route that the player took.
 */
public class GeoJSONManager : MonoBehaviour
{
    [SerializeField]
    private GeoJSONLocation[] locations;

    private GeoJSONLocation GetLocation(string locationName)
    {
        return locations.FirstOrDefault(location => location.location == locationName);
    }

    public List<double[]> GenerateRoute(IEnumerable<Journey> journeys)
    {
        return new List<double[]>(journeys
            .Select(journey => GetLocation(journey.destination))
            .Where(location => location != null)
            .Select(location => new double[] { location.lon, location.lat }));
    }
}
