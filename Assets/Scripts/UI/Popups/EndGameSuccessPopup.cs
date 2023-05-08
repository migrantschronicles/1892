using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameSuccessPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public string TechnicalName { get; set; }

    public void OnEndGameClicked()
    {
        if(string.IsNullOrEmpty(TechnicalName))
        {
            NewGameManager.Instance.EndGameAndReturnToMainMenu();
        }
        else
        {
            LevelInstance.Instance.OpenEndGameDiaryEntry(TechnicalName);
        }
    }

    public void OnDownloadPDFClicked()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
