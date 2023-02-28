using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ShipButton : MonoBehaviour
{
    [SerializeField]
    private GameObject boardPopupPrefab;

    public void OnClick()
    {
        // Go to ship
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
}
