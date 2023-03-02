using System;

public class Leg
{
    //unique identifier, implement ids later
    public string Key => Origin + Destination;

    public string Origin { get; set; }

    public string Destination { get; set; }

    public int Distance { get; set; }

    public TransportationOld Transportation { get; set; }

    public TimeSpan Duration => TimeSpan.FromHours(Distance / Transportation.Speed);
}
