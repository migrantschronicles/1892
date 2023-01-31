using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipIconManager : MonoBehaviour
{

    public bool playerInShip = false;

    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 controlPoint1;

    // Temp variables
    public int daysInCity = 0;
    public int TotalDaysInShip = 10;

    // Start is called before the first frame update
    void Start()
    {
        //OnNewDayDelegate();
       // NewGameManager.Instance.onNewDay += OnNewDayDelegate;
    }

    private void Update()
    {
        if (playerInShip) this.GetComponent<Image>().enabled = true;
        else this.GetComponent<Image>().enabled = false;
   // }

    // Update is called once per frame
    //private void OnNewDayDelegate()
    //{
        if (playerInShip)
        {
            //float t = (NewGameManager.Instance.DaysInCity / TotalDaysInShip); // Make a scale of "days" to a value between 0 and 1
            float t = ((float)(daysInCity) / (float)TotalDaysInShip); // Make a scale of "days" to a value between 0 and 1

            // pointA NewGameManager.Instance.CurrentLocationObject.GetComponent<RectTransform>().anchoredPosition

            // Need to uncomment the line below to allow for movement.
            this.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(Vector3.Lerp(pointA, controlPoint1, t), Vector3.Lerp(controlPoint1, pointB, t), t);
        }
    }

    public void StartShipTravel(Vector3 pointA, Vector3 pointB, Image remaningRoute, Image traveledRoute) 
    {
    
    }

}
