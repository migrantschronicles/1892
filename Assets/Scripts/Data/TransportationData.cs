using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class TransportationData
{
    //todo: estimate speed, luggage space and costs

    //average speed: km/h
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
                TransportationType.StageCoach, 10
            },
            {
                TransportationType.Boat, 15
            },
            {
                TransportationType.TramRail, 15
            },
            {
                TransportationType.Cart, 15
            }
        });

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
                TransportationType.Foot, 1
            },
            {
                TransportationType.Train, 1
            },
            {
                TransportationType.StageCoach, 1
            },
            {
                TransportationType.Boat, 1
            },
            {
                TransportationType.TramRail, 1
            },
            {
                TransportationType.Cart, 1
            }
       });

    public static IEnumerable<Transportation> GetAllTransportation(string origin, string destination)
    {
        return LegData.Legs.Where(l => l.Origin == origin && l.Destination == destination).Select(l => l.Transportation);
    }

    public static IReadOnlyDictionary<string, IEnumerable<TransportationType>> TransportationByLegKey = new ReadOnlyDictionary<string, IEnumerable<TransportationType>>(
        new Dictionary<string, IEnumerable<TransportationType>>()
        {
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
}
