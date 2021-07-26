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
            Origin = CityData.Pfaffenthal,
            Destination = CityData.Luxembourg,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 500
        },
        new Leg
        {
            Origin = CityData.Pfaffenthal,
            Destination = CityData.Luxembourg,
            Transportation = new Transportation
            {
                Type = TransportationType.StageCoach
            },
            Distance = 500
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 500
        },
        new Leg
        {
            Origin = CityData.Luxembourg,
            Destination = CityData.Antwerp,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 500
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
        },
        new Leg
        {
            Origin = CityData.Paris,
            Destination = CityData.Havre,
            Transportation = new Transportation
            {
                Type = TransportationType.Foot
            },
            Distance = 197
        },
         new Leg
        {
            Origin = CityData.Paris,
            Destination = CityData.Havre,
            Transportation = new Transportation
            {
                Type = TransportationType.Train
            },
            Distance = 197
        }
    });

    #endregion

    #region Coordinates

    public static IReadOnlyDictionary<string, IEnumerable<Vector2>> CoordinatesByLegKey = new ReadOnlyDictionary<string, IEnumerable<Vector2>>(
        new Dictionary<string, IEnumerable<Vector2>>()
        {
            {
                CityData.Pfaffenthal + CityData.Luxembourg,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Pfaffenthal],
                    new Vector2(49.760865f, 6.113434f),
                    CityData.LatLonByCity[CityData.Luxembourg]
                }
            },
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
                    new Vector2(49.86277585341321f, 5.537109375000001f),
                    new Vector2(50.28933925329178f, 5.053710937500001f),
                    new Vector2(50.548344490674786f, 4.581298828125001f),
                    CityData.LatLonByCity[CityData.Brussels]
                }
            },
            {
                CityData.Brussels  + CityData.Antwerp,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Brussels],
                    new Vector2(51.048301133312265f, 4.312133789062501f),
                    CityData.LatLonByCity[CityData.Antwerp]
                }
            },
            {
                CityData.Brussels  + CityData.Rotterdam,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Brussels],
                    new Vector2(51.048301133312265f, 4.312133789062501f),
                    CityData.LatLonByCity[CityData.Antwerp],
                    new Vector2(51.49164465653034f, 4.312133789062501f),
                    new Vector2(51.70660846336452f, 4.350585937500001f),
                    CityData.LatLonByCity[CityData.Rotterdam]
                }
            },
            {
                CityData.Antwerp + CityData.Rotterdam,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Antwerp],
                    new Vector2(51.49164465653034f, 4.312133789062501f),
                    new Vector2(51.70660846336452f, 4.350585937500001f),
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
                    new Vector2(49.31796095602274f, 6.212768554687501f),
                    CityData.LatLonByCity[CityData.Metz]
                }
            },
            {
                CityData.Metz + CityData.Paris,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Metz],
                    new Vector2(48.89722676235675f, 5.548095703125f),
                    new Vector2(48.705462895790575f, 5.020751953125f),
                    new Vector2(48.647427805533546f, 4.586791992187501f),
                    new Vector2(48.69458640884518f, 3.6584472656250004f),
                    new Vector2(48.65105695744785f, 2.8070068359375004f),
                    CityData.LatLonByCity[CityData.Paris]
                }
            },
            {
                CityData.Paris + CityData.Havre,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Paris],
                    new Vector2(49.26780455063753f, 1.7907714843750002f),
                    new Vector2(49.224772722794825f, 0.81298828125f),
                    CityData.LatLonByCity[CityData.Havre]
                }
            },
            {
                CityData.Luxembourg + CityData.Arlon,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Luxembourg],
                    new Vector2(49.651626391830476f, 6.001281738281251f),
                    CityData.LatLonByCity[CityData.Arlon]
                }
            },
            {
                CityData.Arlon + CityData.Brussels,
                new List<Vector2>()
                {
                    CityData.LatLonByCity[CityData.Arlon],
                    new Vector2(50.1241000426924f, 5.5810546875f),
                    new Vector2(50.49595785216966f, 5.202026367187501f),
                    new Vector2(50.73297844827752f, 4.812011718750001f),
                    CityData.LatLonByCity[CityData.Brussels]
                }
            }
        });

    #endregion
}
