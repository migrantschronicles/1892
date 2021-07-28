using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartQuitManager : MonoBehaviour
{
    public void StartGame()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        LevelManager.StartLevel("MainMenu");
    }
}
