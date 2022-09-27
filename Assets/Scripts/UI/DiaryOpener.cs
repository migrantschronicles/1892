using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryOpener : MonoBehaviour
{
    public GameObject Diary;
    public MapZoom mapZoom;

    bool isOpened = false;

    public void ChangeState()
    {
        isOpened = !isOpened;
        Diary.SetActive(isOpened);
        if(isOpened)
        {
            mapZoom.ResetInitialZoom();
        }
    }

    public void GoToMainMenu()
    {
        LevelManager.StartLevel("MainMenu");
    }
}
