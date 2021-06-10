using System.Collections.Generic;
using System.Linq;

public class TransportationManager : ITransportationManager
{
    public IEnumerable<Transportation> GetTransportation(string origin, string destination)
    {
        if(!string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(destination))
        {
            if (TransportationData.TransportationByCity.ContainsKey((origin, destination)))
            {
                return TransportationData.TransportationByCity[(origin, destination)];
            }
        }

        return Enumerable.Empty<Transportation>();
    }
}
