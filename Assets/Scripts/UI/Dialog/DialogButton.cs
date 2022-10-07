using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour
{
    [SerializeField]
    private Button dialogButton;
    [SerializeField, Tooltip("The condition under which this dialog button is active")]
    private DialogCondition condition;
    [SerializeField, Tooltip("The scene to open for this dialog, or leave empty for current scene")]
    private string sceneName;
    [SerializeField, Tooltip("The scene which should be layered on top of the blur (the characters left and right)")]
    private string additiveSceneName;
    [SerializeField, Tooltip("The characters involved in the scene (basically everything you want to disappear when the dialog starts).")]
    private GameObject[] hideObjects;

    public string SceneName { get { return sceneName; } }
    public string AdditiveSceneName { get { return additiveSceneName; } }
    public IEnumerable<GameObject> HideObjects { get { return hideObjects; } }

#if UNITY_EDITOR
    private void Validate()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Dialog dialog = transform.GetChild(i).GetComponent<Dialog>();
            if(dialog == null)
            {
                DialogSystem.LogValidateError($"A dialog button should only contain Dialog prefabs as children, not {transform.GetChild(i).name}", 
                    gameObject);
            }
        }

        if(!string.IsNullOrWhiteSpace(sceneName))
        {
            if(!LevelInstance.Instance.HasScene(sceneName))
            {
                DialogSystem.LogValidateError($"The scene '{sceneName}' does not exist", gameObject);
            }
        }

        if(!string.IsNullOrWhiteSpace(additiveSceneName))
        {
            if(!LevelInstance.Instance.HasScene(additiveSceneName))
            {
                DialogSystem.LogValidateError($"The additive scene '{additiveSceneName}' does not exist", gameObject);
            }
        }
    }
#endif

    private void Start()
    {
#if UNITY_EDITOR
        Validate();
#endif
        OnConditionsChanged();
        DialogSystem.Instance.AddOnConditionsChanged(condition.GetAllConditions(), OnConditionsChanged);
        dialogButton.onClick.AddListener(OnStartDialog);
    }

    private void OnConditionsChanged()
    {
        gameObject.SetActive(condition.Test());
    }

    private void OnStartDialog()
    {
        LevelInstance.Instance.StartDialog(this);
    }
}
