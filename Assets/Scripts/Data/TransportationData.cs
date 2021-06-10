using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class TransportationData
{
    //todo: estimate speed, luggage space and costs

    public static IReadOnlyDictionary<TransportationType, int> TransportationSpeedByType =
        new ReadOnlyDictionary<TransportationType, int>(new Dictionary<TransportationType, int>()
        {
            {
                TransportationType.OnFoot, default
            },
            {
                TransportationType.Train, default
            },
            {
                TransportationType.StageCoach, default
            },
            {
                TransportationType.SteamShip, default
            },
        });

    public static IReadOnlyDictionary<(string Origin, string Destination), IEnumerable<Transportation>> TransportationByCity =
        new ReadOnlyDictionary<(string Origin, string Destination), IEnumerable<Transportation>>(new Dictionary<(string Origin, string Destination), IEnumerable<Transportation>>()
        {
            {
                (CityData.Luxembourg, CityData.Antwerp),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    }
                }
            },
            {
                (CityData.Luxembourg, CityData.Arlon),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    }
                }
            },
            {
                (CityData.Arlon, CityData.Antwerp),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    },
                    new Transportation
                    {
                        Type = TransportationType.Train,
                        Speed = TransportationSpeedByType[TransportationType.Train],
                        LuggageSpace = 1,
                        TicketCost = 1
                    }
                }
            },
            {
                (CityData.Luxembourg, CityData.Brussels),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    },
                    new Transportation
                    {
                        Type = TransportationType.Train,
                        Speed = TransportationSpeedByType[TransportationType.Train],
                        LuggageSpace = 1,
                        TicketCost = 1
                    },
                    new Transportation
                    {
                        Type = TransportationType.StageCoach,
                        Speed = TransportationSpeedByType[TransportationType.StageCoach],
                        LuggageSpace = 1,
                        TicketCost = 1
                    }
                }
            },
            {
                (CityData.Brussels, CityData.Antwerp),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    },
                    new Transportation
                    {
                        Type = TransportationType.Train,
                        Speed = TransportationSpeedByType[TransportationType.Train],
                        LuggageSpace = 1,
                        TicketCost = 1
                    }
                }
            },
            {
                (CityData.Antwerp, CityData.Rotterdam),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    },
                    new Transportation
                    {
                        Type = TransportationType.Train,
                        Speed = TransportationSpeedByType[TransportationType.Train],
                        LuggageSpace = 1,
                        TicketCost = 1
                    }
                }
            },
            {
                (CityData.Luxembourg, CityData.Paris),
                new List<Transportation>
                {
                    new Transportation
                    {
                        Type = TransportationType.OnFoot,
                        Speed = TransportationSpeedByType[TransportationType.OnFoot],
                    },
                    new Transportation
                    {
                        Type = TransportationType.Train,
                        Speed = TransportationSpeedByType[TransportationType.Train],
                        LuggageSpace = 1,
                        TicketCost = 1
                    }
                }
            }
        });
}
