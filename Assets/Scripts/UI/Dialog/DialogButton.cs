using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour
{
    [SerializeField]
    private Button dialogButton;
    [SerializeField, Tooltip("The scene to open for this dialog, or leave empty for current scene")]
    private string sceneName;
    [SerializeField, Tooltip("The characters involved in the scene (basically everything you want to disappear when the dialog starts).")]
    private GameObject[] hideObjects;
    [SerializeField, Tooltip("The prefab that will be instantiated on the left side of the dialog.")]
    private GameObject dialogPrefab;
    [SerializeField, Tooltip("The language of the dialoges")]
    private DialogLanguage language = DialogLanguage.Native;

    private bool savedCanStartToday = false;

    public string SceneName { get { return sceneName; } }
    public IEnumerable<GameObject> HideObjects { get { return hideObjects; } }
    public GameObject DialogPrefab { get { return dialogPrefab; } }
    public DialogLanguage Language { get { return language; } }

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

        if(dialogPrefab == null)
        {
            DialogSystem.LogValidateError($"The dialog prefab was not set", gameObject);
        }
    }
#endif

    private void Start()
    {
#if UNITY_EDITOR
        Validate();
#endif
        dialogButton.onClick.AddListener(OnStartDialog);
    }

    private void OnStartDialog()
    {
        if(savedCanStartToday)
        {
            LevelInstance.Instance.StartDialog(this);
            return;
        }

        CharacterHealthData responsibleCharacter = null;
        responsibleCharacter = NewGameManager.Instance.healthStatus.TryStartDialog();

        if(responsibleCharacter == null)
        {
            // The dialog can be started normally.
            savedCanStartToday = true;
            NewGameManager.Instance.onNewDay += OnNewDay;
            LevelInstance.Instance.StartDialog(this);
        }
        else
        {
            // One character is too hungry to start the dialog.
            LevelInstance.Instance.StartTooHungryDialog(this, responsibleCharacter);
        }
    }
    
    private void OnNewDay()
    {
        savedCanStartToday = false;
        NewGameManager.Instance.onNewDay -= OnNewDay;
    }
}
