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
    [SerializeField]
    private ShipIconManager shipMarker;
    [SerializeField]
    private AudioClip selectLocationClip;
    [SerializeField]
    private GameObject pfaffenthalFlag;
    [SerializeField]
    private GameObject weicherdangeFlag;
    [SerializeField]
    private GameObject wormeldangeFlag;

    private MapLocationMarker[] locationMarkers;
    private MapTransportationMethods transportationMethods;

    public MapTransportationMethods TransportationMethods { get { return transportationMethods; } }

    public MapLocationMarker CurrentLocationMarker
    {
        get
        {
            return locationMarkers.FirstOrDefault(marker => marker.LocationName == LevelInstance.Instance.LocationName);
        }
    }

    public MapLocationMarker GetLocationMarkerByName(string name)
    {
        foreach (MapLocationMarker location in locationMarkers) {
            if(location.name == name) return location;
        }
        return null;
    }

    public GameObject CurrentFocusObject
    {
        get
        {
            if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
            {
                return shipMarker.gameObject;
            }
            else if(LevelInstance.Instance.LocationName == "Pfaffenthal")
            {
                return pfaffenthalFlag;
            }
            else if(LevelInstance.Instance.LocationName == "Wormeldange")
            {
                return wormeldangeFlag;
            }
            else if(LevelInstance.Instance.LocationName == "Weicherdange")
            {
                return weicherdangeFlag;
            }

            MapLocationMarker marker = CurrentLocationMarker;
            return marker ? marker.gameObject : null;
        }
    }

    private void Awake()
    {
        locationMarkers = GetComponentsInChildren<MapLocationMarker>();
    }

    private void Start()
    {
        if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
        {
            shipMarker.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        CloseTransportationMethodsImmediately();
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
            StartCoroutine(CloseTransportationMethods(transportationMethods));
            transportationMethods = null;
        }

        AudioManager.Instance.PlayFX(selectLocationClip);

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

    private IEnumerator CloseTransportationMethods(MapTransportationMethods methods)
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
