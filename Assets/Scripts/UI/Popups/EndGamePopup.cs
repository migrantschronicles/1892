using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGamePopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public string TechnicalName { get; set; }

    public void OnEndGameClicked()
    {
        LevelInstance.Instance.OpenEndGameDiaryEntry(TechnicalName);
    }

    public void OnDownloadPDFClicked()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
