using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour
{
    [SerializeField]
    private Button dialogButton;
    [SerializeField, Tooltip("The scene to open for this dialog, or leave empty for current scene")]
    private string sceneName;
    [SerializeField, Tooltip("The characters involved in the scene (basically everything you want to disappear when the dialog starts).")]
    private GameObject[] hideObjects;
    [SerializeField, Tooltip("The language of the dialoges")]
    private DialogLanguage language = DialogLanguage.Native;
    [SerializeField, Tooltip("True if the dialog can start even if the main protagonist is sick (for family members)")]
    private bool canStartEvenIfSick = false;
    [SerializeField]
    private bool canStartEvenIfHungry = false;
    [SerializeField]
    private Sprite defaultButton;
    [SerializeField]
    private Sprite pressedButton;
    [SerializeField]
    private Sprite finishedButton;
    [SerializeField]
    private Sprite ticketButton;
    [SerializeField]
    private Sprite pressedTicketButton;
    [SerializeField]
    private bool isTicketSeller;
    [SerializeField]
    private bool isFinished;

    private bool savedCanStartToday = false;

    public UnityEvent OnDialogEnded;

    public string SceneName { get { return sceneName; } }
    public IEnumerable<GameObject> HideObjects { get { return hideObjects; } }
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
    }
#endif

    private void Start()
    {
#if UNITY_EDITOR
        Validate();
#endif
        UpdateElements();
        dialogButton.onClick.AddListener(OnStartDialog);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateElements();
    }
#endif

    private void OnStartDialog()
    {
        if(savedCanStartToday)
        {
            LevelInstance.Instance.StartDialog(this);
            return;
        }

        ProtagonistHealthData responsibleCharacter = null;
        responsibleCharacter = NewGameManager.Instance.HealthStatus.TryStartDialog(canStartEvenIfSick, canStartEvenIfHungry);

        if(responsibleCharacter == null)
        {
            if(LevelInstance.Instance.TryCanStartDialog())
            {
                // The dialog can be started normally.
                savedCanStartToday = true;
                NewGameManager.Instance.onNewDay += OnNewDay;
                LevelInstance.Instance.StartDialog(this);
            }
            else
            {
                // You can only talk to 2 people a day
                LevelInstance.Instance.StartDailyDialogLimitDialog(this);
            }
        }
        else
        {
            // One character is too hungry to start the dialog or the main character is sick.
            if(responsibleCharacter.CholeraStatus.IsSick && !canStartEvenIfSick)
            {
                // The dialog can't be started because the main character is sick.
                LevelInstance.Instance.StartSickDialog(this, responsibleCharacter.CharacterData);
            }
            else if(responsibleCharacter.CharacterData.isMainProtagonist && responsibleCharacter.HomesickessStatus.Value > 5)
            {
                // The main protagonist is homesick and can only talk to one person.
                LevelInstance.Instance.StartHomesickDialog(this, responsibleCharacter.CharacterData);
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

    private void UpdateElements()
    {
        if(isTicketSeller && Application.isPlaying && NewGameManager.Instance.isHistoryMode)
        {
            gameObject.SetActive(false);
            return;
        }

        ((Image)dialogButton.targetGraphic).sprite = isTicketSeller ? ticketButton : (isFinished ? finishedButton : defaultButton);
        SpriteState state = dialogButton.spriteState;
        Sprite pressedSprite = isTicketSeller ? pressedTicketButton : (isFinished ? finishedButton : pressedButton);
        state.pressedSprite = pressedSprite;
        state.highlightedSprite = pressedSprite;
        dialogButton.spriteState = state;
    }

    public void OnDialogFinished()
    {
        isFinished = true;
        UpdateElements();
        OnDialogEnded?.Invoke();
    }
}
