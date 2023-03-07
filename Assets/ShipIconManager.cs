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

    public List<RectTransform> controlPoints;
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

    public ShipIconRoute GetCurrentRoute()
    {
        return shipRoutes.FirstOrDefault(route => route.from == NewGameManager.Instance.ShipManager.FromLocation);
    }

    // Update is called once per frame
    private void OnNewDayDelegate()
    {
        // Move ship to next point
        GetComponent<RectTransform>().anchoredPosition = GetCurrentRoute().controlPoints[NewGameManager.Instance.DaysInCity].anchoredPosition;
    }
}
