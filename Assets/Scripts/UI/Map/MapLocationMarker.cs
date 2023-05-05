using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LocationMarkerScaling
{
    Default,
    Clamp,
    Map
}

public class MapLocationMarker : MonoBehaviour
{

    //GeoJSON
    [SerializeField]
    public double lat;
    [SerializeField]
    public double lon;

    [SerializeField]
    private Sprite currentMarker;
    [SerializeField]
    private Sprite currentCapitalMarker;
    [SerializeField]
    private Sprite traveledMarker;
    [SerializeField]
    private Sprite traveledCapitalMarker;
    [SerializeField]
    private Sprite discoveredMarker;
    [SerializeField]
    private Sprite discoveredCapitalMarker;
    [SerializeField, Tooltip("Override the location name if it does not match with the game object name")]
    private string overrideLocationName;
    [SerializeField]
    private bool isCapital;
    [SerializeField, Tooltip("The zoom level at which it is considered that the markers have the default scale")]
    private float defaultZoomLevel = 1.0f;
    [SerializeField, Tooltip("The scaling type for the markers: \n" +
        "Default: Markers keep the same screen percentage. ClampMin/-Max and MapMin/-Max is ignored.\n" +
        "Clamp: The zoom level is clamped at the bounds specified by ClampMin/-Max. Useful to not let the markers scale too much.\n" +
        "Map: The zoom level value is remapped to a new range specified by MapMin/-Max. If you not want to let the markers scale to much, but scale always.")]
    private LocationMarkerScaling scaling;
    [SerializeField]
    private float clampMin = 0.5f;
    [SerializeField]
    private float clampMax = 2.0f;
    [SerializeField]
    private float mapMin = 0.5f;
    [SerializeField]
    private float mapMax = 2.0f;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Vector3 initialImageScale;

    private Image image;
    private Button button;
    private MapZoom mapZoom;
    private Map map;

    private Map Map
    {
        get
        {
            if(map == null)
            {
                map = GetComponentInParent<Map>();
            }

            return map;
        }
    }

    public string LocationName
    {
        get
        {
            if(overrideLocationName != null && !string.IsNullOrWhiteSpace(overrideLocationName))
            {
                return overrideLocationName;
            }

            return gameObject.name;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        UpdateName();
        mapZoom = GetComponentInParent<MapZoom>();
        if(mapZoom)
        {
            mapZoom.onMapZoomChangedEvent += OnZoomChanged;
            OnZoomChanged(mapZoom.ZoomLevel);
        }
        UpdateIcon();
        NewGameManager.Instance.OnRouteDiscovered += OnRouteDiscovered;
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.OnRouteDiscovered -= OnRouteDiscovered;
        }
    }

    private void OnRouteDiscovered(string from, string to, TransportationMethod method)
    {
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        LocationDiscoveryStatus status = NewGameManager.Instance.GetDiscoveryStatus(LocationName);
        Sprite sprite = null;
        switch(status)
        {
            case LocationDiscoveryStatus.Discovered: sprite = isCapital ? discoveredCapitalMarker : discoveredMarker; break;
            case LocationDiscoveryStatus.Traveled: sprite = isCapital ? traveledCapitalMarker : traveledMarker; break;
            case LocationDiscoveryStatus.Current: sprite = isCapital ? currentCapitalMarker : currentMarker; break;
        }

        if(sprite != null)
        {
            image.sprite = sprite;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnZoomChanged(float zoomLevel)
    {
        float value = 0.0f;
        switch (scaling)
        {
            case LocationMarkerScaling.Default: value = zoomLevel / defaultZoomLevel; break;
            case LocationMarkerScaling.Clamp: value = Mathf.Clamp(zoomLevel, clampMin, clampMax) / defaultZoomLevel; break;
            case LocationMarkerScaling.Map:
                value = MapZoom.RemapValue(zoomLevel, mapZoom.MinZoomLevel, mapZoom.MaxZoomLevel, mapMin, mapMax) / defaultZoomLevel;
                break;
        }

        transform.localScale = initialImageScale / value;
    }

    private void OnClick()
    {
        Map.OnLocationMarkerClicked(this);
    }

    public void UpdateName()
    {
        nameText.text = NewGameManager.Instance.LocationManager.GetLocalizedName(LocationName);
    }
}
