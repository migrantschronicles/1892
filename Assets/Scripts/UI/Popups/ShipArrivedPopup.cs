using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipArrivedPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public IPopup.OnPopupAction OnLeaveShip;

    public void HandleLeaveShip()
    {
        OnLeaveShip?.Invoke(this);
    }
}
