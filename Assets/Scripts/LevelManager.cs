using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static void StartLevel(string name)
    {
        SceneManager.LoadScene(sceneName: name);
    }

    public static void Quit()
    {
        Application.Quit();       
    }
}
