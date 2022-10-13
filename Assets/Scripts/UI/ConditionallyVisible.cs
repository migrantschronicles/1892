using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionallyVisible : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which the childs of this game object are visible")]
    private DialogCondition condition;

    private void Start()
    {
        OnConditionChanged();
        DialogSystem.Instance.AddOnConditionsChanged(condition.GetAllConditions(), OnConditionChanged);
    }

    private void OnConditionChanged()
    {
        gameObject.SetActive(condition.Test());
    }
}
