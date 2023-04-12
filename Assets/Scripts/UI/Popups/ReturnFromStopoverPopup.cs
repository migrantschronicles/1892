using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnFromStopoverPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }
    public IPopup.OnPopupAction OnBoard;
    public IPopup.OnPopupAction OnStay;


    public void OnBoardClicked()
    {
        OnBoard?.Invoke(this);
    }

    public void OnStayInCityClicked()
    {
        OnStay?.Invoke(this);
    }
}
