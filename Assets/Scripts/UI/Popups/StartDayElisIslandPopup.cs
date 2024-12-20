using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDayElisIslandPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnStartDay;

    public bool CanClose { get { return false; } }

    public void OnAccept()
    {
        OnStartDay?.Invoke(this);
    }
}
