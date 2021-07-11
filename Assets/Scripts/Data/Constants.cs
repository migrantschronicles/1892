using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static DateTime StartDate = new DateTime();

    public static int InitialBalance = 250;

    public static int InitialFood = 5;

    public static int InitialHealth = 100;

    public static int InitialLuggageNumber = 2;

    public static IEnumerable<int> InitialItemIds = new List<int> { 1,2,3,4,5,6,7,8 };
}
