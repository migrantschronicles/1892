using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TransportationRouteInfo 
{
    public string FromLocation;
    public string ToLocation;
    public TransportationMethod method; // Walking, Train, Carriage, etc.
    public float time; // In seconds
    public int cost;
    public int food;
    public bool isDiscoverable;

    /// A flag whether this route was discovered by the player in a dialog.
    public bool WasDiscovered { get; set; }
    /// True if the flag was discovered by the player or is discovered by default.
    public bool IsDiscovered { get { return !isDiscoverable || WasDiscovered; } }

    public override string ToString() 
    {
        return $"Transportation Route Info: {FromLocation}, {ToLocation}, {method}, {time}, {cost}, {food}, {isDiscoverable}";
    }
}


public class TransportationInfoTable
{

    List<TransportationRouteInfo> transportationInfo = new List<TransportationRouteInfo>(); 
    
    public void Initialize(TextAsset infoTable) // infoTable is a CSV file with routes info.
    {
        string fileData = infoTable.text;
        fileData.Replace("\r\n", "\n");
        string[] lines = fileData.Split("\n"[0]);

        for(int i = 1; i < lines.Length; i++) 
        {
            string line = lines[i];
            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] attributes = line.Split(',');
            string FromLocation = attributes[0];
            string ToLocation = attributes[1];
            string type = attributes[2];
            TransportationMethod method = NewGameManager.GetTransportationMethodByName(type);
            float time = float.Parse(attributes[3]);
            int cost = int.Parse(attributes[4]);
            int food = int.Parse(attributes[5]);
            bool discoverable = !string.IsNullOrWhiteSpace(attributes[6]);
            TransportationRouteInfo newRouteInfo = new TransportationRouteInfo 
            { 
                FromLocation = FromLocation, 
                ToLocation = ToLocation, 
                method = method, 
                time = time, 
                cost = cost, 
                food = food,
                isDiscoverable = discoverable
            };

            
            transportationInfo.Add(newRouteInfo);
        }

    }

    public TransportationRouteInfo GetRouteInfo(string FromLocation, string ToLocation, TransportationMethod method) 
    {
        foreach(TransportationRouteInfo route in transportationInfo) 
        {
            if(route.FromLocation == FromLocation && route.ToLocation == ToLocation && route.method == method) 
            {
                return route;
            }
        }

        Debug.LogError($"Route: ({FromLocation}, {ToLocation}, {method}) is not found. Please make sure to include it in the CSV file.");
        return null;
    }

    public bool HasRouteInfo(string from, string to, TransportationMethod method = TransportationMethod.None)
    {
        foreach(TransportationRouteInfo route in transportationInfo)
        {
            if(route.FromLocation == from && route.ToLocation == to)
            {
                if (method == TransportationMethod.None || route.method == method)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void DiscoverRoute(string from, string to, TransportationMethod method)
    {
        foreach(TransportationRouteInfo route in transportationInfo)
        {
            if(route.FromLocation == from && route.ToLocation == to && route.method == method)
            {
                route.WasDiscovered = true;
                return;
            }
        }
    }

    public bool IsRouteDiscovered(string from, string to, TransportationMethod method)
    {
        foreach (TransportationRouteInfo route in transportationInfo)
        {
            if (route.FromLocation == from && route.ToLocation == to && route.method == method)
            {
                return route.IsDiscovered;
            }
        }

        return false;
    }
}
