using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartNewGamePopup : MonoBehaviour, IPopup
{
    public event IPopup.OnPopupAction OnStartGame;
    public event IPopup.OnPopupAction OnDownloadPDF;

    public void OnStartGameClicked()
    {
        OnStartGame?.Invoke(this);
    }

    public void OnDownloadPDFClicked()
    {
        OnDownloadPDF?.Invoke(this);
    }
}
