using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    [SerializeField]
    private float minimumLoadingTime = 2.0f;

    private float startTime = -1.0f;

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        yield return null;

        startTime = Time.time;
        AsyncOperation op = SceneManager.LoadSceneAsync(NewGameManager.Instance.nextLocation);
        op.allowSceneActivation = false;
        while(!op.isDone)
        {
            if(op.progress >= 0.9f)
            {
                if(Time.time - startTime >= minimumLoadingTime)
                {
                    op.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        NewGameManager.Instance.OnTravelComplete();
    }

    /*
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
    */
}
