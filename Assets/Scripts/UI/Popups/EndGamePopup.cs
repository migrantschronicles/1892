using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGamePopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public void OnEndGameClicked()
    {
        NewGameManager.Instance.EndGameAndReturnToMainMenu();
    }

    public void OnDownloadPDFClicked()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
