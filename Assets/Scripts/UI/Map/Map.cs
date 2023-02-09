using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private GameObject locationsParent;

    private MapLocationMarker[] locationMarkers;

    public MapLocationMarker CurrentLocationMarker
    {
        get
        {
            return locationMarkers.FirstOrDefault(marker => marker.LocationName == LevelInstance.Instance.LocationName);
        }
    }

    private void Awake()
    {
        locationMarkers = GetComponentsInChildren<MapLocationMarker>();
    }
}
