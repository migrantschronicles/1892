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
                    NewGameManager.Instance.OnBeforeSceneActivation();
                }
            }

            yield return null;
        }
    }
}
