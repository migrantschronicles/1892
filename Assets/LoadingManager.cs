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
        string loadSceneName = NewGameManager.Instance.nextLocation;
        if(NewGameManager.Instance.ShipManager.IsTravellingInShip)
        {
            if(NewGameManager.Instance.ShipManager.HasReachedDestination)
            {
                // Player reached Elis island.
                loadSceneName = "ElisIsland";
            }
            else if(NewGameManager.Instance.ShipManager.IsStopoverDay)
            {
                if(NewGameManager.Instance.ShipManager.HasVisitedStopover)
                {
                    // Return to the ship.
                    loadSceneName = "Ship";
                }
                else
                {
                    // Player wants to go to stopover.
                    loadSceneName = NewGameManager.Instance.ShipManager.StopoverLocation;
                }
            }
            else
            {
                // Player chose to travel to america
                loadSceneName = "Ship";
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
        op.allowSceneActivation = false;
        while(!op.isDone)
        {
            if(op.progress >= 0.9f)
            {
                if(Time.time - startTime >= minimumLoadingTime)
                {
                    op.allowSceneActivation = true;

                    if(loadSceneName == NewGameManager.Instance.nextLocation)
                    {
                        // Else it is a special map (ship, stopover).
                        NewGameManager.Instance.OnBeforeSceneActivation();
                    }
                    else if(loadSceneName == "Ship")
                    {
                        NewGameManager.Instance.OnLoadedShip();
                    }
                    else if(loadSceneName == NewGameManager.Instance.ShipManager.StopoverLocation)
                    {
                        NewGameManager.Instance.OnLoadedStopover();
                    }
                    else if(loadSceneName == "ElisIsland")
                    {
                        NewGameManager.Instance.OnLoadedElisIsland();
                    }
                }
            }

            yield return null;
        }
    }
}
