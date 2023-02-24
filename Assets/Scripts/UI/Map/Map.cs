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
    private MapTransportationMethods transportationMethods;

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
        if(transportationMethods != null)
        {
            if(transportationMethods.ToLocation == marker.LocationName)
            {
                // Clicks on same marker.
                return;
            }

            // Start anim
            StartCoroutine(CloseTransporationMethods(transportationMethods));
            transportationMethods = null;
        }

        if(!NewGameManager.Instance.CanTravelTo(marker.LocationName) || NewGameManager.Instance.ShipManager.IsTravellingInShip)
        {
            return;
        }

        RectTransform markerTransform = marker.GetComponent<RectTransform>();

        GameObject transportationMethodsGO = Instantiate(transportationMethodsPrefab, transform);
        RectTransform transportationMethodsTransform = transportationMethodsGO.GetComponent<RectTransform>();
        transportationMethodsTransform.anchoredPosition = markerTransform.anchoredPosition;
        transportationMethods = transportationMethodsGO.GetComponent<MapTransportationMethods>();
        transportationMethods.InitMethods(LevelInstance.Instance.LocationName, marker.LocationName);
        transportationMethods.Open();
    }

    private IEnumerator CloseTransporationMethods(MapTransportationMethods methods)
    {
        methods.Close();
        while(!methods.IsClosed())
        {
            yield return 0;
        }
        Destroy(methods.gameObject);
    }

    public void CloseTransportationMethodsImmediately()
    {
        if(transportationMethods != null)
        {
            Destroy(transportationMethods.gameObject);
            transportationMethods = null;
        }
    }
}
