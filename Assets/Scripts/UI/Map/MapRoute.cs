using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRoute : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private string fromLocation;
    [SerializeField]
    private string toLocation;
    [SerializeField]
    private Sprite currentRoute;
    [SerializeField]
    private Sprite traveledRoute;
    [SerializeField]
    private Sprite discoveredRoute;
    [SerializeField]
    private bool bidirectional;

    private void Start()
    {
        UpdateImage();
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
        UpdateImage();
    }

    public void UpdateImage()
    {
        LocationDiscoveryStatus status = NewGameManager.Instance.GetRouteDiscoveryStatus(fromLocation, toLocation);
        if(bidirectional)
        {
            LocationDiscoveryStatus reverseStatus = NewGameManager.Instance.GetRouteDiscoveryStatus(toLocation, fromLocation);
            status = (LocationDiscoveryStatus) Mathf.Max((int) status, (int) reverseStatus);
        }

        Sprite sprite = null;
        switch(status)
        {
            case LocationDiscoveryStatus.Discovered: sprite = discoveredRoute; break;
            case LocationDiscoveryStatus.Traveled: sprite = traveledRoute; break;
            case LocationDiscoveryStatus.Current: sprite = currentRoute; break;
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
}
