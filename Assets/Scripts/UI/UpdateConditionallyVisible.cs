using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateConditionallyVisible : MonoBehaviour
{
    private void Awake()
    {
        NewGameManager.Instance.onNewDay += OnNewDay;
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
        LevelInstance.Instance.StartCoroutine(UpdateContitionallyVisible());
    }

    private IEnumerator UpdateContitionallyVisible()
    {
        yield return null;
        GetComponent<ConditionallyVisible>().UpdateCondition();
    }
}
