using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private GameObject locationsParent;
    [SerializeField]
    private GameObject transportationMethodsPrefab;

    private MapLocationMarker[] locationMarkers;
    private GameObject prevTransportationMethods;
    private GameObject transportationMethodsGO;

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

    public void OnLocationMarkerClicked(MapLocationMarker marker)
    {
        if(transportationMethodsGO != null)
        {
            // Start anim
            ///@todo
            Destroy(transportationMethodsGO);
            transportationMethodsGO = null;
        }

        if(!NewGameManager.Instance.CanTravelTo(marker.LocationName))
        {
            return;
        }

        RectTransform markerTransform = marker.GetComponent<RectTransform>();

        transportationMethodsGO = Instantiate(transportationMethodsPrefab, transform);
        RectTransform transportationMethodsTransform = transportationMethodsGO.GetComponent<RectTransform>();
        transportationMethodsTransform.anchoredPosition = markerTransform.anchoredPosition;
        MapTransportationMethods transportationMethods = transportationMethodsGO.GetComponent<MapTransportationMethods>();
        transportationMethods.InitMethods(LevelInstance.Instance.LocationName, marker.LocationName);
    }
}
