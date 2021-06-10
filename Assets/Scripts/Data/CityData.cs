using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using WPM;

public static class CityData
{
    public const string Belgium = "Belgium";
    public const string France = "France";
    public const string Netherlands = "Netherlands";

    public const string Luxembourg = "Luxembourg";
    public const string Brussels = "Brussels";
    public const string Antwerp = "Antwerp";
    public const string Rotterdam = "Rotterdam";
    public const string Metz = "Metz";
    public const string Arlon = "Arlon";
    public const string Paris = "Paris";

    public static IReadOnlyCollection<string> CityNames = new ReadOnlyCollection<string>(new List<string>()
    {
        Luxembourg,
        Brussels,
        Antwerp,
        Rotterdam,
        Metz,
        Arlon,
        Paris
    });

    public static IReadOnlyDictionary<string, string> CountryByCity = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
    {
        { Luxembourg, Luxembourg },
        { Brussels, Belgium},
        { Antwerp, Belgium },
        { Rotterdam, Netherlands },
        { Metz, France },
        { Arlon, Belgium },
        { Paris, France },
    });

    public static IReadOnlyDictionary<string, Vector2> CityPosition = new ReadOnlyDictionary<string, Vector2>(
       new Dictionary<string, Vector2>()
       {
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
           }
       });

    public static IReadOnlyDictionary<(string, string), IEnumerable<Vector2>> CoordinatesByCity = new ReadOnlyDictionary<(string, string), IEnumerable<Vector2>>(
        new Dictionary<(string, string), IEnumerable<Vector2>>()
        {
            { 
                (Luxembourg, Antwerp), 
                new List<Vector2>() 
                {
                    CityPosition[Luxembourg],
                    CityPosition[Antwerp]
                }
            },
            {
                (Luxembourg, Brussels),
                new List<Vector2>()
                {
                    CityPosition[Luxembourg],
                    CityPosition[Brussels]
                }
            },
            {
                (Brussels, Antwerp),
                new List<Vector2>()
                {
                    CityPosition[Brussels],
                    CityPosition[Antwerp]
                }
            },
            {
                (Antwerp, Rotterdam),
                new List<Vector2>()
                {
                    CityPosition[Antwerp],
                    CityPosition[Rotterdam]
                }
            },
            {
                (Luxembourg, Paris),
                new List<Vector2>()
                {
                    CityPosition[Luxembourg],
                    new Vector2(49.439557f, 6.129303f),
                    new Vector2(49.114632f, 5.679365f),
                    new Vector2(48.999441f, 4.746566f),
                    new Vector2(49.078663f, 4.219809f),
                    new Vector2(49.100248f, 3.616234f),
                    new Vector2(48.818921f, 2.771228f),
                    CityPosition[Paris]
                }
            },
            {
                (Luxembourg, Metz),
                new List<Vector2>()
                {
                    CityPosition[Luxembourg],
                    CityPosition[Metz]
                }
            },
            {
                (Luxembourg, Arlon),
                new List<Vector2>()
                {
                    CityPosition[Luxembourg],
                    //new Vector2(49.439557f, 6.129303f),
                    //new Vector2(49.114632f, 5.679365f),
                    //new Vector2(48.999441f, 4.746566f),
                    //new Vector2(49.078663f, 4.219809f),
                    //new Vector2(49.100248f, 3.616234f),
                    //new Vector2(48.818921f, 2.771228f),
                    CityPosition[Arlon]
                }
            },
            {
                (Arlon, Brussels),
                new List<Vector2>()
                {
                    CityPosition[Arlon],
                    //new Vector2(49.439557f, 6.129303f),
                    //new Vector2(49.114632f, 5.679365f),
                    //new Vector2(48.999441f, 4.746566f),
                    //new Vector2(49.078663f, 4.219809f),
                    //new Vector2(49.100248f, 3.616234f),
                    //new Vector2(48.818921f, 2.771228f),
                    CityPosition[Brussels]
                }
            }
        });
}
