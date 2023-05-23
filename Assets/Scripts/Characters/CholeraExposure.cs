using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
