using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionallyVisible : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which the childs of this game object are visible")]
    private DialogCondition condition;

    private void Start()
    {
        OnConditionChanged(null);
        NewGameManager.Instance.conditions.AddOnConditionsChanged(condition.GetAllConditions(), OnConditionChanged);
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
