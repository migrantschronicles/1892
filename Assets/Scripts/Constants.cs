using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static DateTime StartDate = new DateTime();

    public static int InitialBalance = 1000;

    public static int InitialFood = 10;

    public static int InitialHealth = 100;

    public static int InitialLuggageNumber = 2;

    public static IEnumerable<int> InitialCityIds = new List<int>();

    public static IEnumerable<int> InitialItemIds = new List<int>();

    public static IReadOnlyCollection<TransportationMethod> AvailableMethods = new List<TransportationMethod>() 
    {
        new TransportationMethod("Pfaffenthal", "X", "Luxembourg", "City Center, Immigration Agency", new PathsDetails[] { new PathsDetails("On foot", "Shortest and Steepest", 0.333f, 0, 0), new PathsDetails("On foot", "via rue Vauban", 0.416f, 0, 0), new PathsDetails("On foot", "via côte d’Eich", 0.75f, 0, 0), new PathsDetails("Horse drawn wagon", "Shortest and Steepest", 0.3f, 0, 0), new PathsDetails("Horse drawn wagon", "via rue Vauban", 0.333f, 0, 0), new PathsDetails("Horse drawn wagon", "via côte d’Eich", 0.583f, 0, 0)})
        , new TransportationMethod("Luxembourg", "Immigrant Hotel", "Luxembourg", "Railroad Station", new PathsDetails[] { new PathsDetails("On foot", "X", 0.416f, 0, 0), new PathsDetails("Horse drawn tramrail", "X", 0.333f, 0, 0) })
        , new TransportationMethod("Luxembourg", "X", "Antwerp", "X", new PathsDetails[] { new PathsDetails("On foot", "X", 0.416f, 0, 0) })
        , new TransportationMethod("Luxembourg", "Railroad Station", "Brussels", "X", new PathsDetails[] { new PathsDetails("Train", "X", 6f, 0, 0), new PathsDetails("Stage Coach", "X", 0f, 0, 0) })
        , new TransportationMethod("Brussels", "X", "Antwerp", "X", new PathsDetails[] { new PathsDetails("Train", "X", 1.333f, 0, 0) })
        , new TransportationMethod("Antwerp", "X", "Rotterdam", "X", new PathsDetails[] { new PathsDetails("Train", "X", 1f, 0, 0) })
        , new TransportationMethod("European Ports", "X", "US East Coast", "X", new PathsDetails[] { new PathsDetails("Steam Ship", "X", 240f, 0, 0) })
        , new TransportationMethod("Luxembourg", "X", "Le Havre", "X", new PathsDetails[] { new PathsDetails("Train", "Luxembourg, Paris and Le Havre", 13f, 0, 0) })
    };
}
