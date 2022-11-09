using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{

    void OnEnable() 
    {
        TransportationRouteInfo routeInfo = NewGameManager.Instance.transportationInfo.GetRouteInfo(NewGameManager.Instance.currentLocation, this.GetComponentInParent<LocationMarker>().LocationName, this.transform.name);
        if (routeInfo != null) { 
            float time = routeInfo.time;
            int money = routeInfo.cost;
            int food = routeInfo.food;

            string timeAsString = $"{(int)(time / 86400)}d {(int)((time % 86400) / 3600)}h {(int)((time % 3600)/60)}m";
            

            transform.GetChild(0).GetComponent<TransportationMethodInfo>().time.text = timeAsString;
            transform.GetChild(0).GetComponent<TransportationMethodInfo>().money.text = money.ToString();
            transform.GetChild(0).GetComponent<TransportationMethodInfo>().food.text = food.ToString();

        }
    }

    public void GoToLocation(string name) // Add resources input 
    {
        GetComponentInParent<TransportationButtons>().anim.SetBool("ButtonClicked", false);
        NewGameManager.Instance.GoToLocation(name, this.gameObject.name);
    }
}
