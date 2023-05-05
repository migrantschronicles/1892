using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameSuccessPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public string TechnicalName { get; set; }

    public void OnEndGameClicked()
    {
        LevelInstance.Instance.OpenEndGameDiaryEntry(TechnicalName);
    }

    public void OnDownloadPDFClicked()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        LevelInstance.Instance.UI.OpenDiaryImmediately(DiaryPageLink.Map);
        yield return null;
        NewGameManager.Instance.GeneratePDF();
        LevelInstance.Instance.UI.CloseDiaryImmediately();
    }
}
