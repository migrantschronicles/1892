using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransportationMethods : MonoBehaviour
{
    private MethodManager[] methods;
    private Vector3 initialScale;
    private MapZoom mapZoom;
    private RectTransform rectTransform;

    private void Awake()
    {
        methods = GetComponentsInChildren<MethodManager>();
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;
        mapZoom = GetComponentInParent<MapZoom>();
        mapZoom.onMapZoomChangedEvent += OnZoomChanged;
        OnZoomChanged(mapZoom.ZoomLevel);
    }

    private void OnDestroy()
    {
        if(mapZoom)
        {
            mapZoom.onMapZoomChangedEvent -= OnZoomChanged;
        }
    }

    private void OnZoomChanged(float zoomLevel)
    {
        float scaleFactor = mapZoom.DefaultZoomLevel / zoomLevel;
        rectTransform.localScale = scaleFactor * initialScale;
    }

    public void InitMethods(string from, string to)
    {
        foreach(MethodManager manager in methods)
        {
            if(NewGameManager.Instance.CanTravel(from, to, manager.Method))
            {
                TransportationRouteInfo info = NewGameManager.Instance.transportationInfo.GetRouteInfo(from, to, manager.Method);
                manager.SetRouteInfo(info);
                manager.gameObject.SetActive(true);
            }
            else
            {
                manager.gameObject.SetActive(false);
            }
        }
    }
}
