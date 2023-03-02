public class TransportationOld
{
    public TransportationType Type { get; set; }

    public int Speed => TransportationData.TransportationSpeedByType[Type];

    public int LuggageSpace => TransportationData.TransportationSpaceByType[Type];

    public int TicketCost => TransportationData.TransportationCostByType[Type];
}
