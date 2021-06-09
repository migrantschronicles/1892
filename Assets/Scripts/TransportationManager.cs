using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;


public class PathsDetails
{
    public string method;
    public string path;
    public float time; // Seperate it
    public float distance; // Seperate it
    public float cost; // Seperate it

    public PathsDetails(string method, string path, float time, float distance, float cost){
        this.method = method;
        this.path = path;
        this.time = time;
        this.distance = distance;
        this.cost = cost;
    }

    public void ObjToString()
    {
        Debug.Log(this.method + "," + this.path + "," + this.time + "," + this.distance + "," + this.cost);
    }
}

public class TransportationMethod
{
    public string originCity;
    public string originDetails; // Additional details (Not required) // X means null.
    public string destinationCity;
    public string destinationDetails; // Additional details (Not required) // X means null.
    public PathsDetails[] methodsAndTime;

    public TransportationMethod(string originCity, string originDetails, string destinationCity, string destinationDetails, PathsDetails[] methodsAndTime)
    {
        this.originCity = originCity;
        this.originDetails = originDetails;
        this.destinationCity = destinationCity;
        this.destinationDetails = destinationDetails;
        this.methodsAndTime = methodsAndTime;

        Debug.Log(this);
    }

    public void ObjToString() {
        Debug.Log(this.originCity + "," + this.originDetails + "," + this.destinationCity + "," + this.destinationDetails + "," + this.methodsAndTime);
    }

    
}
    



public class TransportationManager : ITransportationManager
{

    private WorldMapGlobe map;

    IEnumerable<TransportationMethod> ITransportationManager.GetAllTransports()
    {
        return Constants.AvailableMethods;
    }

    IEnumerable<TransportationMethod> ITransportationManager.GetTransportationMethods(int origin_id, int destination_id, WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        this.map = map;


        // Get and return the possible transportation methods from city: origin_id to city: destination_id
        List<TransportationMethod> transportationMethods = new List<TransportationMethod>();

        foreach (TransportationMethod method in Constants.AvailableMethods) {
            if (map.GetCityIndex(method.originCity) == origin_id && map.GetCityIndex(method.destinationCity) == destination_id)
                transportationMethods.Add(method); // Found and added available method.
        }

        return transportationMethods;
    }
}
