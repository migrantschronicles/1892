using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Used in the main menu to load the first scene (Pfaffenthal)
 * and initialize the new game manager to a new game.
 */
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void StartNewGame(string username, bool isHistoryMode)
    {
        StartCoroutine(StartNewGameRoutine(username, isHistoryMode));
    }

    private IEnumerator StartNewGameRoutine(string username, bool isHistoryMode)
    {
        DontDestroyOnLoad(gameObject);

        AudioManager.Instance.FadeOutMusic();
        SceneManager.LoadScene("Pfaffenthal");

        yield return null;

        NewGameManager.Instance.InitNewGame(username, isHistoryMode);

        Destroy(gameObject);
    }

    public static void StartLevel(string name)
    {
        SceneManager.LoadScene(sceneName: name);
    }

    public static void Quit()
    {
        Application.Quit();       
    }
}
