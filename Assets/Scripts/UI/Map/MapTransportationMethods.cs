using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransportationMethods : MonoBehaviour
{
    private MethodManager[] methods;

    private void Awake()
    {
        methods = GetComponentsInChildren<MethodManager>();
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
