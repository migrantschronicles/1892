using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionallyVisible : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which the childs of this game object are visible")]
    private DialogCondition condition;

    private void Start()
    {
        NewGameManager.Instance.conditions.AddOnConditionsChanged(condition.GetAllConditions(), OnConditionChanged);
        LevelInstance.Instance.onSceneChanged += OnSceneChanged;
        OnConditionChanged(null);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.conditions.RemoveOnConditionsChanged(condition.GetAllConditions(), OnConditionChanged);
        }

        if(LevelInstance.Instance)
        {
            LevelInstance.Instance.onSceneChanged -= OnSceneChanged;
        }
    }

    private void OnSceneChanged(Scene scene)
    {
        if(scene.HasInteractable(gameObject))
        {
            UpdateCondition();
        }
    }

    private void OnConditionChanged(object context)
    {
        UpdateCondition();
    }

    public void UpdateCondition()
    {
        gameObject.SetActive(condition.Test());
    }
}
