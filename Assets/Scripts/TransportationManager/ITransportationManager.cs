using System.Collections.Generic;

public interface ITransportationManager
{
    IEnumerable<Transportation> GetTransportation(string origin, string destination);
}
