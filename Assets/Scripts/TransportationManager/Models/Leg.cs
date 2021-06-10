using System;

public class Leg
{
    public string Origin { get; set; }

    public string Destination { get; set; }

    public int Distance { get; set; }

    public Transportation Tranportation { get; set; }

    public TimeSpan Duration => TimeSpan.FromHours(Distance / Tranportation.Speed);
}
