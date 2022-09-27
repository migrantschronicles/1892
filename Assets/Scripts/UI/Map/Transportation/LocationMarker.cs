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

public class LocationMarker : MonoBehaviour
{
    [SerializeField]
    private Image markerImage;
    [SerializeField]
    private GameObject transportationMethodsGO;
    [SerializeField]
    private Color unlockedColor = Color.white;
    [SerializeField]
    private Color lockedColor = new Color(0.7843137255f, 0.7843137255f, 0.7843137255f, 0.5f);
    [SerializeField]
    private bool isUnlocked = false;
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

    private MapZoom mapZoom;
    private Vector3 initialTransportationMethodsScale;
    private Vector3 initialMarkerImageScale;

    private void Start()
    {
        ///@todo The diary prefab should have a script with this as variable, the diary prefab should be accessible via singleton / ...
        mapZoom = GetComponentInParent<MapZoom>();
        mapZoom.onMapZoomChangedEvent += OnZoomChanged;
        initialTransportationMethodsScale = transportationMethodsGO.transform.localScale;
        initialMarkerImageScale = markerImage.transform.localScale;
        SetUnlocked(isUnlocked);
        OnZoomChanged(mapZoom.ZoomLevel);
    }

    public void SetUnlocked(bool unlocked = true)
    {
        isUnlocked = unlocked;
        GetComponent<Button>().interactable = unlocked;
        markerImage.color = unlocked ? unlockedColor : lockedColor;
    }

    private void OnZoomChanged(float zoomLevel)
    {
        float value = 0.0f;
        switch(scaling)
        {
            case LocationMarkerScaling.Default: value = zoomLevel / defaultZoomLevel; break;
            case LocationMarkerScaling.Clamp: value = Mathf.Clamp(zoomLevel, clampMin, clampMax) / defaultZoomLevel; break;
            case LocationMarkerScaling.Map:
                value = MapZoom.RemapValue(zoomLevel, mapZoom.MinZoomLevel, mapZoom.MaxZoomLevel, mapMin, mapMax) / defaultZoomLevel;
                break;
        }

        markerImage.transform.localScale = initialMarkerImageScale / value;
        transportationMethodsGO.transform.localScale = initialTransportationMethodsScale / value;
    }
}
