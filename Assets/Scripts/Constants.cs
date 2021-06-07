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
}
