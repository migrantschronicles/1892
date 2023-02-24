using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{
    [SerializeField]
    private GameObject boardPopupPrefab;

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
            Continent currentContinent = NewGameManager.Instance.LocationManager.GetContinent(routeInfo.FromLocation);
            Continent nextContinent = NewGameManager.Instance.LocationManager.GetContinent(routeInfo.ToLocation);
            if (NewGameManager.Instance.LocationManager.IsFromEuropeToAmerica(routeInfo.FromLocation, routeInfo.ToLocation))
            {
                // Go to ship
                GameObject popupGO = LevelInstance.Instance.ShowPopup(boardPopupPrefab);
                BoardPopup popup = popupGO.GetComponent<BoardPopup>();
                popup.OnStayInCity += (_) => 
                {
                    LevelInstance.Instance.PopPopup();
                    // Notify map to close methods
                    GetComponentInParent<Map>().CloseTransportationMethodsImmediately();
                };
                popup.OnBoard += (_) =>
                {
                    LevelInstance.Instance.PopPopup();
                    NewGameManager.Instance.GoToLocation(routeInfo.ToLocation, Method);
                };
            }
            else
            {
                // Travel inside continent.
                NewGameManager.Instance.GoToLocation(routeInfo.ToLocation, Method);
            }
        }
    }
}
