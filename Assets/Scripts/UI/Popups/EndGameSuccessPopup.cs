using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameSuccessPopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnEndGame;
    public event IPopup.OnPopupAction OnDownloadPDF;

    public bool CanClose { get { return false; } }

    public void OnEndGameClicked()
    {
        OnEndGame?.Invoke(this);
    }

    public void OnDownloadPDFClicked()
    {
        OnDownloadPDF?.Invoke(this);
    }
}
