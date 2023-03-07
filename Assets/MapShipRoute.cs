using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapShipRoute : MonoBehaviour
{

    [SerializeField]
    private Image traveledRouteimage;
    [SerializeField]
    private string fromLocation;
    [SerializeField]
    private GameObject currentRoute;
    [SerializeField]
    private ShipIconManager shipIconManager; // To retrieve control points
    [SerializeField]
    private GameObject firstRoute; // If exists, should indicate there are two routes and handles them accordingly.

    private void Start()
    {
        if (NewGameManager.Instance.ShipManager.FromLocation == fromLocation && NewGameManager.Instance.ShipManager.IsTravellingInShip) 
        {
            NewGameManager.Instance.onNewDay += UpdateTravelRoute;
            UpdateTravelRoute();
        }
        else
        {
            this.gameObject.SetActive(false);
        }

    }

    private void OnDestroy()
    {
        if (NewGameManager.Instance != null) // Might need to revisit; if there are warnings.
            NewGameManager.Instance.onNewDay -= UpdateTravelRoute;
    }

    public void UpdateTravelRoute() 
    {
        RectTransform currentCP = shipIconManager.GetCurrentRoute().controlPoints[NewGameManager.Instance.DaysInCity];
        RectTransform traveledRouteRT = traveledRouteimage.GetComponent<RectTransform>();

        Vector3 localPoint = currentCP.InverseTransformPoint(traveledRouteRT.position);

        traveledRouteimage.GetComponent<Image>().fillAmount = currentCP.GetComponent<MapShipControlPoint>().fillAmount;  //((localPoint.x / currentCP.sizeDelta.x) + 0.5f);

        /*if(NewGameManager.Instance.DaysInCity )
        traveledRouteimage.GetComponent<Image>().fillAmount = (float)(NewGameManager.Instance.DaysInCity / 10);*/
    }

}
