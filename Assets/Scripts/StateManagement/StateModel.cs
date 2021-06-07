using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateModel
{
    public DateTime StartDate { get; set; }

    public TimeSpan ElapsedTime { get; set; }

    //todo: create a separate health model to include children's state
    public int CurrentHealth { get; set; }

    public int AvailableMoney { get; set; }

    public int AvailableFood { get; set; }

    public int LuggageNumber { get; set; }

    public int CurrentCityId { get; set; }

    public int PreviousCityId { get; set; }

    public IEnumerable<int> AvailableCityIds { get; set; }

    public IEnumerable<int> VisitedCityIds { get; set; }

    public IEnumerable<int> AvailableItemIds { get; set; }
}
