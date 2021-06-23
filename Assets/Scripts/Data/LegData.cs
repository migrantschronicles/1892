using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class LegData
{
    #region Legs

    public static IReadOnlyCollection<Leg> Legs = new ReadOnlyCollection<Leg>(new List<Leg>() 
    { 
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 300
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 250
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Arlon,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 26
        },
        new Leg
        {
            Origin = CityData.Arlon,
            Destination = CityData.Brussels,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 195
        },
        new Leg
        {
            Origin = CityData.Brussels,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 70
        },
        new Leg
        {
            Origin = CityData.Brussels,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 55
        },
        new Leg
        {
            Origin = CityData.Brussels,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 55
        },
        new Leg
        {
            Origin = CityData.Antwerp,
            Destination = CityData.Rotterdam,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 110
        },
        new Leg
        {
            Origin = CityData.Antwerp,
            Destination = CityData.Rotterdam,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 99
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Brussels,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 187
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Brussels,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 195
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Brussels,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 195
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Paris,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 500
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Paris,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 497
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Metz,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 70
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Metz,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 65
        },
        new Leg
        {
            Origin = CityData.Metz,
            Destination = CityData.Paris,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 495
        },
        new Leg
        {
            Origin = CityData.Metz,
            Destination = CityData.Paris,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 489
        }
    });

    #endregion

    #region Coordinates

    public static IReadOnlyDictionary<string, IEnumerable<Vector2>> CoordinatesByLegKey = new ReadOnlyDictionary<string, IEnumerable<Vector2>>(
        new Dictionary<string, IEnumerable<Vector2>>()
        {
            {
                CityData.Luxembourg + CityData.Antwerp,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    new Vector2(49.955933f, 6.086071f),
                    new Vector2(50.195657f, 5.895854f),
                    new Vector2(50.392178f, 5.687346f),
                    new Vector2(50.731820f, 5.471523f),
                    new Vector2(50.898414f, 5.273989f),
                    new Vector2(51.085123f, 5.076455f),
                    new Vector2(51.119616f, 4.769180f),
                    CityData.LatLonByCity[CityData.Antwerp]
                }
            },
            {
                CityData.Luxembourg + CityData.Brussels,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    new Vector2(49.590535f, 5.938688f),
                    new Vector2(49.746964f, 5.406444f),
                    new Vector2(49.899352f, 5.077221f),
                    new Vector2(50.199173f, 4.940045f),
                    new Vector2(50.308053f, 5.174745f),
                    new Vector2(50.537872f, 4.280484f),
                    CityData.LatLonByCity[CityData.Brussels]
                }
            },
            {
                CityData.Brussels  + CityData.Antwerp,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Brussels],
                    new Vector2(50.990710f, 4.232159f),
                    new Vector2(51.141448f, 4.222194f),
                    CityData.LatLonByCity[CityData.Antwerp]
                }
            },
            {
                CityData.Antwerp + CityData.Rotterdam,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Antwerp],
                    new Vector2(51.515580f, 4.639210f),
                    new Vector2(51.737235f, 4.644697f),
                    CityData.LatLonByCity[CityData.Rotterdam]
                }
            },
            {
                CityData.Luxembourg + CityData.Paris,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    new Vector2(49.439557f, 6.129303f),
                    new Vector2(49.114632f, 5.679365f),
                    new Vector2(48.999441f, 4.746566f),
                    new Vector2(49.078663f, 4.219809f),
                    new Vector2(49.100248f, 3.616234f),
                    new Vector2(48.818921f, 2.771228f),
                    CityData.LatLonByCity[CityData.Paris]
                }
            },
            {
                CityData.Luxembourg + CityData.Metz,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    CityData.LatLonByCity[CityData.Metz]
                }
            },
            {
                CityData.Metz + CityData.Paris,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Metz],
                    CityData.LatLonByCity[CityData.Paris]
                }
            },
            {
                CityData.Luxembourg + CityData.Arlon,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    CityData.LatLonByCity[CityData.Arlon]
                }
            },
            {
                CityData.Arlon + CityData.Brussels,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Arlon],
                    CityData.LatLonByCity[CityData.Brussels]
                }
            }
        });

    #endregion
}
