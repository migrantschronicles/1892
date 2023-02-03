using Articy.Unity;
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
    private GameObject leftDialogPrefab;
    [SerializeField, Tooltip("The prefab that will be instantiated on the right side of the dialog. If null, then it uses the default one.")]
    private GameObject rightDialogPrefab;
    [SerializeField, Tooltip("The language of the dialoges")]
    private DialogLanguage language = DialogLanguage.Native;
    [SerializeField, Tooltip("True if the dialog can start even if the main protagonist is sick (for family members)")]
    private bool canStartEvenIfSick = false;

    private bool savedCanStartToday = false;

    public string SceneName { get { return sceneName; } }
    public IEnumerable<GameObject> HideObjects { get { return hideObjects; } }
    public GameObject LeftDialogPrefab { get { return leftDialogPrefab; } }
    public GameObject RightDialogPrefab { get { return rightDialogPrefab; } }
    public DialogLanguage Language { get { return language; } }
    public DialogChat Chat { get; set; }

#if UNITY_EDITOR
    private void Validate()
    {
        if(!string.IsNullOrWhiteSpace(sceneName))
        {
            if(!LevelInstance.Instance.HasScene(sceneName))
            {
                Debug.LogError($"The scene '{sceneName}' does not exist in {name}");
            }
        }

        if(leftDialogPrefab == null)
        {
            Debug.LogError($"The left dialog prefab was not set in {name}");
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

        ProtagonistHealthData responsibleCharacter = null;
        responsibleCharacter = NewGameManager.Instance.HealthStatus.TryStartDialog(canStartEvenIfSick);

        if(responsibleCharacter == null)
        {
            // The dialog can be started normally.
            savedCanStartToday = true;
            NewGameManager.Instance.onNewDay += OnNewDay;
            LevelInstance.Instance.StartDialog(this);
        }
        else
        {
            // One character is too hungry to start the dialog or the main character is sick.
            if(responsibleCharacter.CholeraStatus.IsSick && !canStartEvenIfSick)
            {
                // The dialog can't be started because the main character is sick.
                LevelInstance.Instance.StartSickDialog(this);
            }
            else
            {
                // The dialog can't be started because one family member is too hungry.
                LevelInstance.Instance.StartTooHungryDialog(this, responsibleCharacter.CharacterData);
            }
        }
    }
    
    private void OnNewDay()
    {
        savedCanStartToday = false;
        NewGameManager.Instance.onNewDay -= OnNewDay;
    }
}
