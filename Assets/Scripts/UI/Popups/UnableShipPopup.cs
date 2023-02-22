using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnableShipPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnGoBack;

    public bool CanClose { get { return false; } }

    public void OnGoBackClicked()
    {
        OnGoBack?.Invoke(this);
    }
}
