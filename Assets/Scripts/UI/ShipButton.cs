using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ShipButton : MonoBehaviour
{
    [SerializeField]
    private GameObject boardPopupPrefab;
    [SerializeField]
    private GameObject returnFromStopoverPrefab;
    [SerializeField]
    private GameObject unableShipPrefab;

    public void OnClick()
    {
        // Go to ship
        if (NewGameManager.Instance.ShipManager.IsStopoverDay)
        {
            // The player is visiting the stopover, so return to the ship.
            GameObject popupGO = LevelInstance.Instance.ShowPopup(returnFromStopoverPrefab);
            ReturnFromStopoverPopup popup = popupGO.GetComponent<ReturnFromStopoverPopup>();
            popup.OnStay += (_) =>
            {
                LevelInstance.Instance.PopPopup();
            };
            popup.OnBoard += (_) =>
            {
                LevelInstance.Instance.PopPopup();
                LevelInstance.Instance.OnReturnFromStopover(false);
            };
        }
        else
        {
            // The first time the player is boarding the ship.
            bool canTravel = NewGameManager.Instance.HealthStatus.CanTravel();
            if (canTravel)
            {
                GameObject popupGO = LevelInstance.Instance.ShowPopup(boardPopupPrefab);
                BoardPopup popup = popupGO.GetComponent<BoardPopup>();
                popup.OnStayInCity += (_) =>
                {
                    LevelInstance.Instance.PopPopup();
                };
                popup.OnBoard += (_) =>
                {
                    LevelInstance.Instance.PopPopup();
                    NewGameManager.Instance.GoToLocation("NewYorkCity", TransportationMethod.Ship);
                };
            }
            else
            {
                GameObject popupGO = LevelInstance.Instance.ShowPopup(unableShipPrefab);
                UnableShipPopup popup = popupGO.GetComponent<UnableShipPopup>();
                popup.OnGoBack += (_) =>
                {
                    LevelInstance.Instance.PopPopup();
                };
            }
        }
    }
}
