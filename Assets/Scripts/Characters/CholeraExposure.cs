using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Exploses the protagonists on level start (if you travel to the city) and on each new day (if you sleep there).
 * Should be added to the LevelInstance game object of the levels in which characters can be exposed to cholera.
 */
public class CholeraExposure : MonoBehaviour
{
    private void Start()
    {
        NewGameManager.Instance.onNewDay += OnNewDay;
        ExposeCharacters();
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }

    private void OnNewDay()
    {
        ExposeCharacters();
    }

    private void ExposeCharacters()
    {
        foreach (var healthData in NewGameManager.Instance.HealthStatus.Characters)
        {
            healthData.CholeraStatus.OnExposed();
        }
    }
}
