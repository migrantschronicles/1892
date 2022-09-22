using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogButton : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which this dialog button is active")]
    private DialogCondition condition;

    private void Start()
    {
        OnConditionsChanged();
        DialogSystem.Instance.AddOnConditionsChanged(condition.GetAllConditions(), OnConditionsChanged);
    }

    private void OnConditionsChanged()
    {
        gameObject.SetActive(condition.Test());
    }
}
