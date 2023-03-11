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

    private void UpdateImage()
    {
        LocationDiscoveryStatus status = NewGameManager.Instance.GetRouteDiscoveryStatus(fromLocation, toLocation);
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
