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

    public CharacterType SelectedCharacter { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void StartNewGame(string username, CharacterType character, bool isHistoryMode)
    {
        StartCoroutine(StartNewGameRoutine(username, character, isHistoryMode));
    }

    private IEnumerator StartNewGameRoutine(string username, CharacterType character, bool isHistoryMode)
    {
        DontDestroyOnLoad(gameObject);
        SelectedCharacter = character;

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
