using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnBoard;
    public event IPopup.OnPopupAction OnStayInCity;

    public bool CanClose { get { return false; } }

    public void OnBoardClicked()
    {
        OnBoard?.Invoke(this);
    }

    public void OnStayInCityClicked()
    {
        OnStayInCity?.Invoke(this);
    }
}
