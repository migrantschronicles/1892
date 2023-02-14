using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{
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
            NewGameManager.Instance.GoToLocation(routeInfo.ToLocation, Method);
        }
    }
}
