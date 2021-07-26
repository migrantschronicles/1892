using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryOpener : MonoBehaviour
{
    public GameObject Diary;

    bool isOpened = false;

    public void ChangeState()
    {
        isOpened = !isOpened;
        Diary.SetActive(isOpened);
    }

    public void GoToMainMenu()
    {
        LevelManager.StartLevel("MainMenu");
    }
}
