using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannotTravelAgainTodayPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnAccept;

    public bool CanClose { get { return false; } }

    public void HandleAccept()
    {
        OnAccept?.Invoke(this);
    }
}
