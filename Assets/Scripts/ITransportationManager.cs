using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITransportationManager
{
    // Define transportation methods
    public IEnumerable<TransportationMethod> GetAllTransports();

    public IEnumerable<TransportationMethod> GetTransportationMethods(int origin_id, int destination_id);
}
