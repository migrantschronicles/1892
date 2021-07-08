using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public static class TransportationData
{
    //Average Speed: km/h
    public static IReadOnlyDictionary<TransportationType, int> TransportationSpeedByType =
        new ReadOnlyDictionary<TransportationType, int>(new Dictionary<TransportationType, int>()
        {
            {
                TransportationType.Foot, 5
            },
            {
                TransportationType.Train, 30
            },
            {
                TransportationType.StageCoach, 15
            },
            {
                TransportationType.Boat, 17
            },
            {
                TransportationType.TramRail, 18
            },
            {
                TransportationType.Cart, 16
            }
        });

    //Luggage Space
    public static IReadOnlyDictionary<TransportationType, int> TransportationSpaceByType =
       new ReadOnlyDictionary<TransportationType, int>(new Dictionary<TransportationType, int>()
       {
            {
                TransportationType.Foot, 1
            },
            {
                TransportationType.Train, 3
            },
            {
                TransportationType.StageCoach, 2
            },
            {
                TransportationType.Boat, 3
            },
            {
                TransportationType.TramRail, 2
            },
            {
                TransportationType.Cart, 2
            }
       });

    public static IReadOnlyDictionary<TransportationType, int> TransportationCostByType =
       new ReadOnlyDictionary<TransportationType, int>(new Dictionary<TransportationType, int>()
       {
            {
                TransportationType.Foot, 0
            },
            {
                TransportationType.Train, 45
            },
            {
                TransportationType.StageCoach, 51
            },
            {
                TransportationType.Boat, 70
            },
            {
                TransportationType.TramRail, 46
            },
            {
                TransportationType.Cart, 33
            }
       });

    public static TimeSpan GetDuration(TransportationType type, float distance)
    {
        return TimeSpan.FromHours(distance / TransportationSpaceByType[type]);
    }

    public static IEnumerable<Transportation> GetAllTransportation(string origin, string destination)
    {
        return LegData.Legs.Where(l => l.Origin == origin && l.Destination == destination).Select(l => l.Transportation);
    }

    public static IReadOnlyDictionary<string, IEnumerable<TransportationType>> TransportationByLegKey = new ReadOnlyDictionary<string, IEnumerable<TransportationType>>(
        new Dictionary<string, IEnumerable<TransportationType>>()
        {
            {
                CityData.Pfaffenthal + CityData.Luxembourg,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.StageCoach
                }
            },
            {
                CityData.Luxembourg + CityData.Antwerp,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Luxembourg + CityData.Brussels,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Brussels  + CityData.Antwerp,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Antwerp + CityData.Rotterdam,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Luxembourg + CityData.Paris,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Luxembourg + CityData.Metz,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Metz + CityData.Paris,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Luxembourg + CityData.Arlon,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            },
            {
                CityData.Arlon + CityData.Brussels,
                new List<TransportationType>()
                {
                    TransportationType.Foot,
                    TransportationType.Train,
                    TransportationType.StageCoach,
                    TransportationType.TramRail,
                    TransportationType.Cart
                }
            }
        });

    public static IReadOnlyDictionary<TransportationType, (string Name, Vector2 Size)> TransportationIconByType =
      new ReadOnlyDictionary<TransportationType, (string, Vector2)>(new Dictionary<TransportationType, (string, Vector2)>()
      {
            {
                TransportationType.Foot, ("foot_icon", new Vector2(35, 35))
            },
            {
                TransportationType.Train, ("train_icon", new Vector2(35, 35))
            },
            {
                TransportationType.StageCoach, ("carriage_icon", new Vector2(35, 35))
            },
            {
                TransportationType.Boat, ("boat_icon", new Vector2(35, 35))
            },
            {
                TransportationType.TramRail, ("tram_icon", new Vector2(35, 35))
            },
            {
                TransportationType.Cart, ("carriage_icon", new Vector2(35, 35))
            }
      });
}
