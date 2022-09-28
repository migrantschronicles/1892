using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogButton : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which this dialog button is active")]
    private DialogCondition condition;
    [SerializeField, Tooltip("The scene to open for this dialog")]
    private string sceneName;

    public string SceneName { get { return sceneName; } }

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
