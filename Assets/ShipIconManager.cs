using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShipIconRoute
{
    public GameObject routeParent;
    public string from;
}

public class ShipIconManager : MonoBehaviour
{
    public ShipIconRoute[] shipRoutes;

    // Start is called before the first frame update
    void Start()
    {
        OnNewDayDelegate();
        NewGameManager.Instance.onNewDay += OnNewDayDelegate;
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onNewDay -= OnNewDayDelegate;
        }
    }

    private ShipIconRoute GetCurrentRoute()
    {
        return shipRoutes.FirstOrDefault(route => route.from == NewGameManager.Instance.ShipManager.FromLocation);
    }

    // Update is called once per frame
    private void OnNewDayDelegate()
    {
        ShipIconRoute currentRoute = GetCurrentRoute();
        Vector3 pointA = currentRoute.routeParent.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        Vector3 controlPoint1 = currentRoute.routeParent.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition;
        Vector3 pointB = currentRoute.routeParent.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition;

        float t = (float)NewGameManager.Instance.DaysInCity / NewGameManager.Instance.ShipManager.TravelDays;

        // Need to uncomment the line below to allow for movement.
        GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(Vector3.Lerp(pointA, controlPoint1, t), Vector3.Lerp(controlPoint1, pointB, t), t);
    }
}
