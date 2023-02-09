using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{

    private bool loadInitiated = false;
    private string loadLevel;

    // Update is called once per frame
    void Update()
    {
        if (!loadInitiated) 
        {
            loadLevel = LevelInstance.Instance.LocationName;
            LoadNextLevel(loadLevel);
            loadInitiated = true;
        }
    }

    public void LoadNextLevel(string name) 
    {
        StartCoroutine(WaitXSeconds(3));
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName: name);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if(scene.name == loadLevel)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            NewGameManager.Instance.PostLevelLoad();
        }
    }

    public IEnumerator WaitXSeconds(float seconds) 
    {
        yield return new WaitForSeconds(seconds);
    }
}
