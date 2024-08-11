using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokePopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnContinue;
    public event IPopup.OnPopupAction OnDownloadPDF;
    public event IPopup.OnPopupAction OnMainMenu;

    public void OnContinueClicked()
    {
        OnContinue?.Invoke(this);
    }

    public void OnDownloadPDFClicked()
    {
        OnDownloadPDF?.Invoke(this);
    }

    public void OnMainMenuClicked()
    {
        OnMainMenu?.Invoke(this);
    }
}
