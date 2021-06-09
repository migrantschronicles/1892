using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;

public interface ITransportationManager
{
    // Define transportation methods
    public IEnumerable<TransportationMethod> GetAllTransports();

    public IEnumerable<TransportationMethod> GetTransportationMethods(int origin_id, int destination_id, WorldMapGlobe map);
}
