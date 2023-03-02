using System.Collections.Generic;

public interface ITransportationManager
{
    IEnumerable<TransportationOld> GetTransportation(string origin, string destination);
}
