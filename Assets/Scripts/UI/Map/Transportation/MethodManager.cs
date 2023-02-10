using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{
    private TransportationMethodInfo methodInfo;
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
        methodInfo = GetComponentInChildren<TransportationMethodInfo>();
    }

    public void SetRouteInfo(TransportationRouteInfo info)
    {
        routeInfo = info;
        float time = info.time;
        int money = info.cost;
        int food = info.food;
        string timeAsString = $"{(int)(time / 86400)}d {(int)((time % 86400) / 3600)}h {(int)((time % 3600) / 60)}m";
        methodInfo.time.text = timeAsString;
        methodInfo.money.text = money.ToString();
        methodInfo.food.text = food.ToString();
    }

    public void GoToDestination()
    {
        if(routeInfo != null)
        {
            NewGameManager.Instance.GoToLocation(routeInfo.ToLocation, Method);
        }
    }
}
