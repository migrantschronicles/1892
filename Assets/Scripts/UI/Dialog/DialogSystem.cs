using Articy.TheMigrantsChronicles;
using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DialogLanguage
{
    Native,
    English,
    Italian
}

/**
 * The dialog system that controls the dialogs in one level. 
 * This is attached to the scroll view and exists once per level (does not use DontDestroyOnLoad).
 * The idea is to separate the data (which text to display) from the actual UI (the bubbles).
 * The data is implemented as monobehaviors so you can drag prefabs in the outliner and parent them in the hierarchy.
 * This is for simplicity and not having to write a custom editor, since structuring them as ScriptableObjects only would be messy with more complex dialogs.
 * Generally it does not matter where the prefabs (dialog data) reside, but to be consistent it should be as childs from the button that triggers the dialog.
 * E.g. A person "Katrin" has a button to start a dialog, so all dialog data (prefabs) should be a child of that button.
 * 
 * DIALOG BUTTON
 * This is a prefab you can drag into the world which can trigger a dialog.
 * The dialog data for this specific person should be a child of this button.
 * You can set a condition and the dialog button will only be enabled if the conditions are met (in the ConditionallyVisible component).
 * You can use this e.g. to hide a dialog button after you have talked to a person, so you can't talk to him again.
 * You can also set a dialog language for people that are talking in different languages and you need to buy a dictionary to be able to understand them.
 * You can hide objects when a dialog of this dialog button is played from the scene.
 * You can use this to hide e.g. the person you're talking to, so he is not visible anymore in the background (because you see him on the left side
 * of the dialog above the blur).
 * You have to set the dialog prefab that is used to display the npc you're talking to on the left side of the screen (this should be one of Assets/Characters).
 * The prefab will be instatiated above the blur once you start the dialog.
 * See the documentation in LevelInstance > Add a dialog button for more information.
 * 
 * CONDITIONS
 * A condition is a string (could be any string you choose).
 * For example a decision could set a condition. 
 * Dialogs can set that they only play if certain conditions are met, so you can control which dialogs play after which decision.
 * You can set it globally (persistent after level change) or locally (will be lost after a new level loads).
 * A dialog can set multiple conditions (strings) as requirements and set that it will only play if either all or at least one condition is met.
 * An empty condition is considered to be true.
 * You can negate each condition so that it only plays if the condition is not met.
 * E.g. If you do not want to play a dialog, add one condition with an empty string and enable Not.
 * Refer to the documentation of DialogConditionProvider for more information.
 */
public class DialogSystem : MonoBehaviour, IPointerClickHandler, IScriptMethodProvider
{
    public delegate void OnConditionsChanged();

    public static DialogSystem Instance { get; private set; }
    public bool IsCalledInForecast { get; set; }

    [SerializeField]
    private GameObject chatPrefab;
    [SerializeField, Tooltip("The time for each character in a text animation")]
    private float timeForCharacters = 0.1f;
    [SerializeField, Tooltip("Whether the talk animation should be played only once or the whole time a dialog bubble is active")]
    private bool talkOnce;
    [SerializeField]
    private GameObject discoveredRoutePopup;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip lineClip;
    public AudioClip decisionOptionClip;

    public delegate void OnDialogLineEvent(string speakerTechnicalName);
    public event OnDialogLineEvent onDialogLine;
    public delegate void OnDialogDecisionEvent();
    public event OnDialogDecisionEvent onDialogDecision;

    private GameObject content;
    private ArticyFlowPlayer flowPlayer;
    private DialogChat currentChat;
    private DialogButton currentButton;
    private Dictionary<IAnimatedText, TextElementAnimator> animators = new();

    public ArticyFlowPlayer FlowPlayer { get { return flowPlayer; } }
    public GameObject DiscoveredRoutePopup { get { return discoveredRoutePopup; } }

    private void Awake()
    {
        Instance = this;
        ScrollRect scrollView = GetComponent<ScrollRect>();
        content = scrollView.content.gameObject;
        flowPlayer = GetComponent<ArticyFlowPlayer>();
        // set the default method provider for script methods, so that we needn't pass it as a parameter when calling script methods manually.
        // look into region "script methods" at the end of this class for more information.
        ArticyDatabase.DefaultMethodProvider = this;
    }

    /**
     * Called when the back button is pressed.
     * @return True if the dialog should actually close, false otherwise.
     */
    public bool OnClose()
    {
        FinishAnimators();

        if(currentChat)
        {
            if(!currentChat.OnClosing())
            {
                return false;
            }

            CloseCurrentChat();
        }

        return true;
    }

    private void OpenDialog(DialogButton button)
    {
        CloseCurrentChat();

        if (!button.Chat)
        {
            // Create a chat object if it does not exist.
            GameObject chatGO = Instantiate(chatPrefab, content.transform);
            button.Chat = chatGO.GetComponent<DialogChat>();
        }

        currentButton = button;
        currentChat = button.Chat;
        currentChat.gameObject.SetActive(true);
        currentChat.OnHeightChanged += OnChatHeightChanged;
        OnChatHeightChanged(currentChat.Height);
    }

    /**
     * Starts a dialog.
     * Goes through each child of the parent and plays the first dialog that meets its conditions.
     * The parent only should have Dialogs as children.
     */
    public void StartDialog(DialogButton button, DialogLanguage language)
    {
        OpenDialog(button);

        // Find the first dialog that matches.
        for(int i = 0; i < button.transform.childCount; ++i)
        {
            Dialog dialog = button.transform.GetChild(i).GetComponent<Dialog>();
            if(dialog != null)
            {
                if(dialog.Condition.Test())
                {
                    currentChat.Play(dialog);
                    break;
                }
            }
        }
    }

    public void StartDialog(DialogButton button, IArticyObject specialDialog)
    {
        OpenDialog(button);
        currentChat.PlaySpecial(specialDialog);
    }

    private void CloseCurrentChat()
    {
        if(currentChat != null)
        {
            FinishAnimators();

            currentChat.OnHeightChanged -= OnChatHeightChanged;
            currentChat.gameObject.SetActive(false);
            currentChat.OnClosing();
            currentChat = null;
            currentButton = null;
        }
    }

    private void OnChatHeightChanged(float height)
    {
        ///@todo Check if the chat that changed is the current visible
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, height);
        contentTransform.anchoredPosition = new Vector2(contentTransform.anchoredPosition.x, Mathf.Max(0, height - 800));
    }

    /// <summary>
    /// This is one of the important callbacks from the ArticyFlowPlayer, and will notify us about pausing on any flow object.
    /// It will make sure that the paused object is displayed in our dialog ui, by extracting its text, potential speaker etc.
    /// </summary>
    public void OnFlowPlayerPaused(IFlowObject flowObject)
    {
        // if the flow player paused on a dialog, we immediately continue, usually getting to the first dialogue fragment inside the dialogue
        // makes it more convenient to set the startOn to a dialogue
        if (flowObject is IDialogue)
        {
            // Don't enable this, because then the corresponding OnBranchesUpdated won't be called and the dialog starts on the second fragment.
            //flowPlayer.Play();
            return;
        }

        if (currentChat)
        {
            currentChat.OnFlowPlayerPaused(flowObject);
        }
    }

    /// <summary>
    /// This is the other important callback from the ArticyFlowPlayer, and is called everytime the flow player has new branches
    /// for us. We use that to update the list of buttons in our dialog interface.
    /// </summary>
    public void OnBranchesUpdated(IList<Branch> branches)
    {
        if(flowPlayer.PausedOn is IDialogue)
        {
            // The dialog is paused on the dialog (first fragment), so continue to 
            // the first dialog fragment.
            ///@todo Handle multiple branches
            Debug.Assert(branches.Count == 1);
            StartCoroutine(SetStartOnDelayed(branches[0].Target as IArticyObject));
            return;
        }

        if (currentChat)
        {
            currentChat.OnBranchesUpdated(branches);
        }
    }

    private IEnumerator SetStartOnDelayed(IArticyObject targetObject)
    {
        yield return new WaitForEndOfFrame();
        flowPlayer.StartOn = targetObject;
    }

    public string ConditionallyEstrangeLine(string text)
    {
        if(!currentButton)
        {
            return text;
        }

        if(!NewGameManager.Instance.UnderstandsDialogLanguage(currentButton.Language))
        {
            // Estrange the text
            text = NewGameManager.Instance.EstrangeText(text);
        }

        return text;
    }

    /**
     * Called when the back button was pressed during an overlay (when a shop or diary was opened during dialog).
     */
    public void OnOverlayClosed()
    {
        if(currentChat)
        {
            currentChat.OnOverlayClosed();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if there are animators
        if(animators.Count > 0)
        {
            // Finish the animators
            FinishAnimators();
            AudioManager.Instance.PlayCutTypewriter();
            return;
        }

        if(currentChat)
        {
            currentChat.OnPointerClick();
        }
    }

    public bool IsRight(IFlowObject flowObject)
    {
        string technicalName = GetTechnicalNameOfSpeaker(flowObject);
        if(technicalName != null)
        {
            return IsRight(technicalName);
        }

        return false;
    }

    public bool IsRight(string technicalName)
    {
        return LevelInstance.Instance.HasRightForegroundCharacter(technicalName);
    }

    public string GetTechnicalNameOfSpeaker(IFlowObject flowObject)
    {
        var dlgSpeaker = flowObject as IObjectWithSpeaker;
        if (dlgSpeaker != null)
        {
            // getting the speaker object
            var speaker = dlgSpeaker.Speaker;
            if (speaker != null)
            {
                return speaker.TechnicalName;
            }
        }

        return null;
    }

    public void RegisterAnimator(IAnimatedText animatedText, string text)
    {
        UnregisterAnimator(animatedText);

        TextElementAnimator newAnimator = new TextElementAnimator(this, animatedText, text, timeForCharacters);
        animators.Add(animatedText, newAnimator);
        newAnimator.onFinished += (animator) =>
        {
            TextElementAnimator textAnimator = (TextElementAnimator)animator;
            animators.Remove(textAnimator.AnimatedTextInterface);
        };

        newAnimator.Start();
    }

    public bool UnregisterAnimator(IAnimatedText animatedText)
    {
        if (animators.TryGetValue(animatedText, out var oldAnimator))
        {
            oldAnimator.Finish();
            animators.Remove(animatedText);
            return true;
        }

        return false;
    }

    public void FinishAnimators()
    {
        foreach (var animator in animators)
        {
            animator.Value.Finish();
        }
        animators.Clear();
    }

    public bool IsCurrentBranch(DialogAnswerBubble bubble)
    {
        if(currentChat)
        {
            return currentChat.IsCurrentBranch(bubble);
        }

        return false;
    }

    public void OnDialogLine(string technicalName)
    {
        NewGameManager.Instance.HealthStatus.OnDialogLine(IsRight(technicalName));
        onDialogLine?.Invoke(technicalName);
    }

    public void OnDialogDecision()
    {
        NewGameManager.Instance.HealthStatus.OnDialogDecision();
        onDialogDecision?.Invoke();
    }
}
