using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void StartLevel(string name)
    {
        SceneManager.LoadScene(sceneName: name);
    }

    public void Quit()
    {
        Application.Quit();       
    }
}
