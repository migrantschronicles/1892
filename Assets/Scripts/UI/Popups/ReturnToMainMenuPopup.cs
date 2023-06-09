using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMainMenuPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnBack;
    public event IPopup.OnPopupAction OnMainMenu;

    public void OnBackClicked()
    {
        OnBack?.Invoke(this);
    }

    public void OnMainMenuClicked()
    {
        OnMainMenu?.Invoke(this);
    }
}
