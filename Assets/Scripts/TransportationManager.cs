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
    



public class TransportationManager : MonoBehaviour, ITransportationManager
{
    public List<TransportationMethod> availableMethods = new List<TransportationMethod>(); // Would contain all available routes,methods,etc.
    PathsDetails[] paths; // Re-usable variable

    public WorldMapGlobe map;

    public void Start() {

        // *** Initialization of all cities paths and methods *** //

        // Pfaffenthal <TO> Luxembourg //
        paths = new PathsDetails[6];
        PathsDetails path1 = new PathsDetails("On foot", "Shortest and Steepest", 0.333f, 0, 0);
        PathsDetails path2 = new PathsDetails("On foot", "via rue Vauban", 0.416f, 0, 0);
        PathsDetails path3 = new PathsDetails("On foot", "via côte d’Eich", 0.75f, 0, 0);
        PathsDetails path4 = new PathsDetails("Horse drawn wagon", "Shortest and Steepest", 0.3f, 0, 0);
        PathsDetails path5 = new PathsDetails("Horse drawn wagon", "via rue Vauban", 0.333f, 0, 0);
        PathsDetails path6 = new PathsDetails("Horse drawn wagon", "via côte d’Eich", 0.583f, 0, 0);
        paths[0] = path1;
        paths[1] = path2;
        paths[2] = path3;
        paths[3] = path4;
        paths[4] = path5;
        paths[5] = path6;
        TransportationMethod Luxembourg1 = new TransportationMethod("Pfaffenthal", "X", "Luxembourg", "City Center, Immigration Agency",paths);
        availableMethods.Add(Luxembourg1);

        Luxembourg1.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(Luxembourg1.originCity));
        Debug.Log(map.GetCityIndex(Luxembourg1.destinationCity));
        for (int i = 0; i < paths.Length; i++) {
            paths[i].ObjToString();
        }

        // Immigrant Hotel, Luxembourg <TO> Railroad Station, Luxembourg (Also?) //
        paths = new PathsDetails[2];
        PathsDetails path7 = new PathsDetails("On foot", "X", 0.416f, 0, 0);
        PathsDetails path8 = new PathsDetails("Horse drawn tramrail", "X", 0.333f, 0, 0);
        paths[0] = path7;
        paths[1] = path8;
        TransportationMethod Luxembourg2 = new TransportationMethod("Luxembourg", "Immigrant Hotel", "Luxembourg", "Railroad Station", paths);
        availableMethods.Add(Luxembourg2);

        Luxembourg2.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(Luxembourg2.originCity));
        Debug.Log(map.GetCityIndex(Luxembourg2.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // Luxembourg <TO> Antwerp Harbor //
        paths = new PathsDetails[1];
        PathsDetails path9 = new PathsDetails("On foot", "X", 0.416f, 0, 0);
        paths[0] = path9;
        TransportationMethod LuxembourgToAntwerp = new TransportationMethod("Luxembourg", "X", "Antwerp", "X", paths);
        availableMethods.Add(LuxembourgToAntwerp);

        LuxembourgToAntwerp.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(LuxembourgToAntwerp.originCity));
        Debug.Log(map.GetCityIndex(LuxembourgToAntwerp.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // Luxembourg <TO> Brussels //
        paths = new PathsDetails[2];
        PathsDetails path10 = new PathsDetails("Train", "X", 6f, 0, 0);
        PathsDetails path11 = new PathsDetails("Stage Coach", "X", 0f, 0, 0);
        paths[0] = path10;
        paths[1] = path11;
        TransportationMethod LuxembourgToBrussels = new TransportationMethod("Luxembourg", "Railroad Station", "Brussels", "X", paths);
        availableMethods.Add(LuxembourgToBrussels);

        LuxembourgToBrussels.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(LuxembourgToBrussels.originCity));
        Debug.Log(map.GetCityIndex(LuxembourgToBrussels.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // Brussels <TO> Antwerp //
        paths = new PathsDetails[1];
        PathsDetails path12 = new PathsDetails("Train", "X", 1.333f, 0, 0);
        paths[0] = path12;
        TransportationMethod BrusselsToAntwerp = new TransportationMethod("Brussels", "X", "Antwerp", "X", paths);
        availableMethods.Add(BrusselsToAntwerp);

        BrusselsToAntwerp.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(BrusselsToAntwerp.originCity));
        Debug.Log(map.GetCityIndex(BrusselsToAntwerp.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // Antwerp <TO> Rotterdam //
        paths = new PathsDetails[1];
        PathsDetails path13 = new PathsDetails("Train", "X", 1f, 0, 0);
        paths[0] = path13;
        TransportationMethod AntwerpToRotterdam = new TransportationMethod("Antwerp", "X", "Rotterdam", "X", paths);
        availableMethods.Add(AntwerpToRotterdam);

        AntwerpToRotterdam.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(AntwerpToRotterdam.originCity));
        Debug.Log(map.GetCityIndex(AntwerpToRotterdam.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // European Ports <TO> US East Coast //
        paths = new PathsDetails[1];
        PathsDetails path14 = new PathsDetails("Steam Ship", "X", 240f, 0, 0);
        paths[0] = path14;
        // The below line should be adjusted according to location of european ports as well as the destination city in US.
        TransportationMethod EuropePortsToUSEC = new TransportationMethod("European Ports", "X", "US East Coast", "X", paths); 
        availableMethods.Add(EuropePortsToUSEC);

        EuropePortsToUSEC.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(EuropePortsToUSEC.originCity));
        Debug.Log(map.GetCityIndex(EuropePortsToUSEC.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // Luxembourg <TO> Paris and Le Havre //
        paths = new PathsDetails[1];
        PathsDetails path15 = new PathsDetails("Train", "Luxembourg, Paris and Le Havre", 13f, 0, 0);
        paths[0] = path15;
        TransportationMethod LuxToParisHavre = new TransportationMethod("Luxembourg", "X", "Le Havre", "X", paths);
        availableMethods.Add(LuxToParisHavre);

        LuxToParisHavre.ObjToString();
        // To test these cities exist.
        Debug.Log(map.GetCityIndex(LuxToParisHavre.originCity));
        Debug.Log(map.GetCityIndex(LuxToParisHavre.destinationCity));
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].ObjToString();
        }

        // **************************************** // 

    }

    IEnumerable<TransportationMethod> ITransportationManager.GetAllTransports()
    {
        return availableMethods;
    }

    IEnumerable<TransportationMethod> ITransportationManager.GetTransportationMethods(int origin_id, int destination_id)
    {
        // Get and return the possible transportation methods from city: origin_id to city: destination_id
        List<TransportationMethod> transportationMethods = new List<TransportationMethod>();

        foreach (TransportationMethod method in availableMethods) {
            if (map.GetCityIndex(method.originCity) == origin_id && map.GetCityIndex(method.destinationCity) == destination_id)
                transportationMethods.Add(method); // Found and added available method.
        }

        return transportationMethods;
    }
}
