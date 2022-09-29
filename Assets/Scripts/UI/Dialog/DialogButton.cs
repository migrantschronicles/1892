using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogButton : MonoBehaviour
{
    [SerializeField, Tooltip("The condition under which this dialog button is active")]
    private DialogCondition condition;
    [SerializeField, Tooltip("The scene to open for this dialog, or leave empty for current scene")]
    private string sceneName;
    [SerializeField, Tooltip("The scene which should be layered on top of the blur (the characters left and right)")]
    private string additiveSceneName;

    public string SceneName { get { return sceneName; } }
    public string AdditiveSceneName { get { return additiveSceneName; } }

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
