using System;
using System.Collections.Generic;

public class State
{
    public DateTime StartDate { get; set; }

    public TimeSpan ElapsedTime { get; set; }

    //todo: create a separate health model to include children's state
    public int CurrentHealth { get; set; }

    public int AvailableMoney { get; set; }

    public int AvailableFood { get; set; }

    public int LuggageNumber { get; set; }

    public string CurrentCityName { get; set; }

    public string PreviousCityName { get; set; }

    public IEnumerable<string> AvailableCityNames { get; set; }

    public IEnumerable<string> VisitedCityNames { get; set; }

    public IEnumerable<int> AvailableItemIds { get; set; }
}
