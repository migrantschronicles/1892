using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        float value = zoomLevel / defaultZoomLevel;
        markerImage.transform.localScale = initialMarkerImageScale / value;
        transportationMethodsGO.transform.localScale = initialTransportationMethodsScale / value;
    }
}
