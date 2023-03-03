using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip selectMethodClip;

    private TransportationMethodBox methodBox;
    private TransportationRouteInfo routeInfo;

    public TransportationMethod Method
    {
        get
        {
            return NewGameManager.GetTransportationMethodByName(gameObject.name);
        }
    }

    private void Awake()
    {
        methodBox = GetComponentInChildren<TransportationMethodBox>();
    }

    public void SetRouteInfo(TransportationRouteInfo info)
    {
        routeInfo = info;
        methodBox.RouteInfo = info;
    }

    public void GoToDestination()
    {
        if(routeInfo != null)
        {
            AudioManager.Instance.PlayFX(selectMethodClip);
            if (!NewGameManager.Instance.LocationManager.IsFromEuropeToAmerica(routeInfo.FromLocation, routeInfo.ToLocation))
            {
                // Travel inside continent.
                NewGameManager.Instance.GoToLocation(routeInfo.ToLocation, Method);
            }
        }
    }
}
