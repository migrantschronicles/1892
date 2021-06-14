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
                TransportationType.SteamShip, 15
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
                TransportationType.SteamShip, 3
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
                TransportationType.SteamShip, 1
            }
       });

    public static IEnumerable<Transportation> GetAllTransportation(string origin, string destination)
    {
        return LegData.Legs.Where(l => l.Origin == origin && l.Destination == destination).Select(l => l.Transportation);
    }
}
