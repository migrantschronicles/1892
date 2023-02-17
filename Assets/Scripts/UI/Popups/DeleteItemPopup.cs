using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteItemPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnAccepted;
    public event IPopup.OnPopupAction OnRejected;

    public void OnAccept()
    {
        OnAccepted?.Invoke(this);
    }

    public void OnReject()
    {
        OnRejected?.Invoke(this);
    }
}
