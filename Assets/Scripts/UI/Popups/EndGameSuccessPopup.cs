using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameSuccessPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public void OnEndGameClicked()
    {
        LevelInstance.Instance.ShowEndGameEntry();
    }

    public void OnDownloadPDFClicked()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
