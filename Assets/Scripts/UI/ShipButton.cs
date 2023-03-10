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

    public void OnClick()
    {
        // Pause the game, otherwise we would need to handle what happens if the player has the popup open too long.
        NewGameManager.Instance.SetPaused(true);

        // Go to ship
        if (NewGameManager.Instance.ShipManager.IsStopoverDay)
        {
            // The player is visiting the stopover, so return to the ship.
            GameObject popupGO = LevelInstance.Instance.ShowPopup(returnFromStopoverPrefab);
            ReturnFromStopoverPopup popup = popupGO.GetComponent<ReturnFromStopoverPopup>();
            popup.OnStay += (_) =>
            {
                NewGameManager.Instance.SetPaused(false);
                LevelInstance.Instance.PopPopup();
            };
            popup.OnBoard += (_) =>
            {
                NewGameManager.Instance.SetPaused(false);
                LevelInstance.Instance.PopPopup();
                LevelInstance.Instance.OnReturnFromStopover(false);
            };
        }
        else
        {
            // The first time the player is boarding the ship.
            GameObject popupGO = LevelInstance.Instance.ShowPopup(boardPopupPrefab);
            BoardPopup popup = popupGO.GetComponent<BoardPopup>();
            popup.OnStayInCity += (_) =>
            {
                NewGameManager.Instance.SetPaused(false);
                LevelInstance.Instance.PopPopup();
            };
            popup.OnBoard += (_) =>
            {
                NewGameManager.Instance.SetPaused(false);
                LevelInstance.Instance.PopPopup();
                NewGameManager.Instance.GoToLocation("NewYorkCity", TransportationMethod.Ship);
            };
        }
    }
}
