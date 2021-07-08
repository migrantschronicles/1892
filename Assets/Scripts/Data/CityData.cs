using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

//Cities that are involved in the prototype
public static class CityData
{
    //Countries
    public const string Belgium = "Belgium";
    public const string France = "France";
    public const string Netherlands = "Netherlands";

    //Cities
    public const string Pfaffenthal = "Pfaffenthal";
    public const string Luxembourg = "Luxembourg";
    public const string Brussels = "Brussels";
    public const string Antwerp = "Antwerp";
    public const string Rotterdam = "Rotterdam";
    public const string Metz = "Metz";
    public const string Arlon = "Arlon";
    public const string Paris = "Paris";
    public const string Havre = "Havre";

    public static IReadOnlyDictionary<string, string> CountryByCity = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
    {
        { Pfaffenthal, Luxembourg },
        { Luxembourg, Luxembourg },
        { Brussels, Belgium},
        { Antwerp, Belgium },
        { Rotterdam, Netherlands },
        { Metz, France },
        { Arlon, Belgium },
        { Paris, France },
        { Havre, France }
    });

    public static IReadOnlyDictionary<string, Vector2> LatLonByCity = new ReadOnlyDictionary<string, Vector2>(
       new Dictionary<string, Vector2>()
       {
           {
               Pfaffenthal, new Vector2(49.738830f, 6.237488f)
           },
           {
               Luxembourg, new Vector2(49.61171f, 6.129978f)
           },
           {
               Brussels, new Vector2(50.83335f, 4.333288f)
           },
           {
               Antwerp, new Vector2(51.22038f, 4.415013f)
           },
           {
               Rotterdam, new Vector2(51.91997f, 4.479932f)
           },
           {
               Metz, new Vector2(49.1193f, 6.1757f)
           },
           {
               Arlon, new Vector2(49.6851f, 5.8105f)
           },
           {
               Paris, new Vector2(48.8566f, 2.3522f)
           },
           {
               Havre, new Vector2(49.4938975f, 0.1079732f)
           }
       });
}
