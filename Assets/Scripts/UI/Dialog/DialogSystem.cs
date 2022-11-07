using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
 * You can set a condition and the dialog button will only be enabled if the conditions are met.
 * You can use this e.g. to hide a dialog button after you have talked to a person, so you can't talk to him again.
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
 * 
 * DIALOG ELEMENT
 * There are multiple elements that can be added to the hierarchy: Dialog, DialogLine, DialogDecision, DialogDecisionOption, DialogRedirector and DialogSelector.
 * Note that within a dialog, only prefabs of a DialogElement should be added, and only correctly 
 * (e.g. only place a DialogDecisionOption as a direct child of a DialogDecision, place Dialog only as a root of the dialog button).
 * 
 * DIALOG
 * A dialog is a parent container for all data (dialog bubbles) that will be displayed.
 * This should only be as a root node (e.g. The child of the dialog button that triggers the dialog).
 * If you call DialogSystem.StartDialog with a GameObject, it will go through all child dialogs (ordered) and start the first dialog that meets its conditions.
 * It does not go through all dialogs sequentially after one dialog finishes, only the first dialog that meets its condition is played.
 * If you have other child game objects in the button, you can add an empty game object to the button and use that as a parent to the dialogs.
 * If you call DialogSystem.StartDialog with a single dialog object, it will not check if it meets the conditions, but rather play it without checking.
 * You can have callbacks, when the dialog is finished (OnFinished). This is called when the last line of a dialog is played or the dialog was left
 * because of a redirector.
 * Also you can set conditions if a dialog is finished.
 * 
 * DIALOG LINE
 * A dialog line represents one dialog bubble.
 * You can set the text that should be displayed and whether it should be a bubble on the left (npc) or right (player).
 * You can also add conditions that should be set when the line plays.
 * E.g. on the last line of a dialog, you can set a condition which signals that the dialog was played.
 * Then you can add another dialog (e.g. "I already talked to you") which plays if the main dialog already played.
 * 
 * DIALOG DECISION
 * A decision is a decision the player can make.
 * This is the parent container for any DialogDecisionOption that the player can take.
 * Note that no element after a decision can be played, since following dialog elements should be a child of each answer,
 * so a decision is the last element in the same level of hierarchy.
 * It can set conditions that will be set if any answer is selected.
 * 
 * DIALOG DECISION OPTION
 * An option for a decision that the player can select.
 * A child of a DialogDecision.
 * You can set the text that should be displayed and select which answer type it is. Depending on the answer type the next action will be done automatically
 * (e.g. AnswerType.Items: automatically opens inventory to trade items).
 * If the answer type is Items or Quest, you need to select the shop that you want to open (should be placed in the Overlays of the LevelInstance).
 * If autoTriggerAction is enabled, selecting this option will immediately trigger the action (opening the shop / the diary map).
 * If it is disabled, you need to place a DialogTriggerLastOption when you want to trigger it.
 * You can use this, if you want to have the shop opened, but you want to have one or more dialog lines first before opening the shop.
 * The dialog will continue after the back button of the shop / diary was pressed.
 * You can also set the conditions that should be added if this specific option is chosen.
 * You can also add conditions that must be met so that the option is displayed in the first place.
 * This can be useful if you do not have an option anymore because of some action on another level.
 * You can also add EnabledCondition. This lets you disable an option if a condition is not met.
 * You can use this e.g. if you want to trade items, but the player does not have a required item.
 * Then you can add a SetCondition in the item, and enable the option only if the player has the item in the inventory.
 * 
 * DIALOG TRIGGER LAST OPTION
 * Triggers the action of the last decision option that was selected.
 * Useful if you e.g. want to open the diary map after a Travel option, but want to have a few lines before really opening the map.
 * 
 * DIALOG SELECTOR
 * A selector is a parent for dialog elements that should only be displayed if certain conditions meet.
 * Normally, after every element of the selector is played, the next element after the selector is played.
 * You can also have nested selectors.
 * Note that if the selector plays a redirector or or a decision, no following elements will be played.
 *
 * DIALOG REDIRECTOR
 * A redirector redirects to another dialog.
 * This is useful if e.g. you have a decision, where each option could have one or more individual dialog lines, but then you want to redirect
 * to a dialog if the rest of the dialog is the same, so you don't have to duplicate the data.
 * Note that after a redirector, no element of the original dialog is played, so it should be the last element of its level in the hierarchy
 * and is the last element of a dialog that is played (if the redirector is played).
 */
public class DialogSystem : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnConditionsChanged();

    public static DialogSystem Instance { get; private set; }

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject answerPrefab;
    [SerializeField, Tooltip("The vertical space between each bubble")]
    private float spacing = 30;
    [SerializeField, Tooltip("The time for each character in a text animation")]
    private float timeForCharacters = 0.1f;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip lineClip;
    public AudioClip decisionOptionClip;

    private GameObject content;
    private List<string> conditions = new List<string>();

    private Dialog currentDialog;
    private DialogElement currentElement;
    private DialogBubble currentBubble;
    private DialogDecision currentDecision;
    private DialogDecisionOption lastSelectedOption;
    private float currentY = 0;
    private List<DialogAnswerBubble> currentAnswers = new List<DialogAnswerBubble>();
    private List<ElementAnimator> currentAnimators = new List<ElementAnimator>();
    private Dictionary<string, OnConditionsChanged> onConditionsChangedListeners = new Dictionary<string, OnConditionsChanged>();

    private void Awake()
    {
        Instance = this;
        ScrollRect scrollView = GetComponent<ScrollRect>();
        content = scrollView.content.gameObject;
    }

    private void Activate()
    {
        gameObject.SetActive(true);
    }

    public void OnClose()
    {
        ResetState();
        ClearContent();
    }

    /**
     * Starts a dialog.
     * Goes through each child of the parent and plays the first dialog that meets its conditions.
     * The parent only should have Dialogs as children.
     */
    public void StartDialog(GameObject parent)
    {
        StartDialog(parent, false);
    }

    /**
     * Starts a dialog.
     * Goes through each child of the parent and plays the first dialog that meets its conditions.
     * The parent only should have Dialogs as children.
     * @param additive If true, adds the dialog below the existing dialog bubbles. If false, clears all existing dialog bubbles.
     */
    public void StartDialog(GameObject parent, bool additive)
    {
        Activate();
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            Dialog dialog = parent.transform.GetChild(i).GetComponent<Dialog>();
            if(dialog.Condition.Test())
            {
                StartDialog(dialog, additive);
                return;
            }
        }
    }

    /**
     * Plays a specific dialog.
     * Does not check if the dialog meets its conditions.
     */
    public void StartDialog(Dialog dialog)
    {
        StartDialog(dialog, false);
    }

    /**
     * Plays a specific dialog.
     * Does not check if the dialog meets its conditions.
     * @param additive If true, adds the dialog below the existing dialog bubbles. If false, clears all existing dialog bubbles.
     */
    public void StartDialog(Dialog dialog, bool additive)
    {
        if(currentDialog)
        {
            OnDialogFinished();
        }

        Activate();
        if (!additive)
        {
            ClearContent();
            currentY = 0;
        }

        ResetState();
        currentDialog = dialog;
        EnterElementContainer(currentDialog);
    }

    private void OnDialogFinished()
    {
        if(currentDialog)
        {
            currentDialog.OnFinished.Invoke();
            AddConditions(currentDialog.SetOnFinishedConditions);
            currentDialog = null;
        }
    }

    private void ResetState()
    {
        foreach (ElementAnimator animator in currentAnimators)
        {
            animator.Finish();
        }
        currentAnimators.Clear();

        currentDialog = null;
        currentElement = null;
        currentBubble = null;
        currentDecision = null;
        currentAnswers.Clear();
        lastSelectedOption = null;
    }

    private bool IsLastLine(DialogLine line)
    {
        DialogElement parent = line.transform.parent.GetComponent<DialogElement>();
        DialogElement current = line;
        while (parent)
        {
            for (int i = current.transform.GetSiblingIndex() + 1; i < parent.transform.childCount; ++i)
            {
                DialogElement e = parent.transform.GetChild(i).GetComponent<DialogElement>();
                switch(e.Type)
                {
                    case DialogElementType.Line:
                    case DialogElementType.Decision:
                    case DialogElementType.Selector:
                    case DialogElementType.Redirector:
                    case DialogElementType.TriggerLastOption:
                        return false;
                }
            }

            if (parent.Type == DialogElementType.Dialog)
            {
                // Don't trace further.
                break;
            }

            current = parent;
            parent = parent.transform.parent.GetComponent<DialogElement>();
        }

        return true;
    }

    private void EnterElementContainer(DialogElement parent)
    {
        if(parent.transform.childCount == 0)
        {
            // There are no childs to process
            OnDialogFinished();
            currentElement = null;
            return;
        }

        currentElement = parent.transform.GetChild(0).GetComponent<DialogElement>();
        ProcessElement(currentElement);
    }

    private bool ProcessNextElement()
    {
        if(currentElement)
        {
            int nextIndex = currentElement.transform.GetSiblingIndex() + 1;
            if(nextIndex < currentElement.transform.parent.childCount)
            {
                // Process the next sibling element.
                currentElement = currentElement.transform.parent.GetChild(nextIndex).GetComponent<DialogElement>();
                ProcessElement(currentElement);
                return true;
            }
            else
            {
                // Check if the current element has a selector as parent, in that case continue with the siblings of the selector.
                // Do in a loop if the parent selector has no next element, but is itself a child of a selector.
                DialogSelector parentSelector = currentElement.transform.parent.GetComponent<DialogSelector>();
                bool processed = false;
                while(parentSelector && !processed)
                {
                    int nextParentIndex = parentSelector.transform.GetSiblingIndex() + 1;
                    if(nextParentIndex < parentSelector.transform.parent.childCount)
                    {
                        // Process the next sibling of the parent selector and jump out of the loop.
                        currentElement = parentSelector.transform.parent.GetChild(nextParentIndex).GetComponent<DialogElement>();
                        ProcessElement(currentElement);
                        processed = true;
                        return true;
                    }
                    else
                    {
                        // The parent selector has no next elements, so try the parent of the parent selector.
                        parentSelector = parentSelector.transform.parent.GetComponent<DialogSelector>();
                    }
                }
            }
        }

        return false;
    }

    private void ProcessElement(DialogElement element)
    {
        switch(element.Type)
        {
            case DialogElementType.Line:
            {
                ProcessLine((DialogLine)element);
                break;
            }

            case DialogElementType.Decision:
            {
                ProcessDecision((DialogDecision)element);
                break;
            }

            case DialogElementType.Redirector:
            {
                ProcessRedirector((DialogRedirector)element);
                break;
            }

            case DialogElementType.Selector:
            {
                ProcessSelector((DialogSelector)element);
                break;
            }

            case DialogElementType.TriggerLastOption:
            {
                ProcessTriggerLastOption((DialogTriggerLastOption)element);
                break;
            }
        }
    }

    private void OnContentAdded(GameObject newContent)
    {
        // Position the new content to the current y value.
        RectTransform rectTransform = newContent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -currentY);
        // Add the height of the new content to the current y value.
        currentY += rectTransform.rect.height;
        // Set the size of the scroll rect to the new height (including the new content).
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY);
        // Add the spacing to the current y value, so the next new content will be placed slightly below.
        currentY += spacing;

        // Set the position of the scroll rect to scroll to the new content.
        Canvas.ForceUpdateCanvases();
        float newY = ((Vector2)transform.InverseTransformPoint(contentTransform.position) -
            (Vector2)transform.InverseTransformPoint(newContent.transform.position)).y;
        contentTransform.anchoredPosition = new Vector2(contentTransform.anchoredPosition.x, newY);
    }

    private void ProcessLine(DialogLine line)
    {
        GameObject newLine = Instantiate(linePrefab, content.transform);
        currentBubble = newLine.GetComponent<DialogBubble>();
        currentBubble.SetContent(line);
        AddConditions(line.SetConditions);
        OnContentAdded(newLine);
        StartTextAnimation(currentBubble, LocalizationManager.Instance.GetLocalizedString(line.Text));
        AudioManager.Instance.PlayFX(lineClip);

        if(IsLastLine(line))
        {
            OnDialogFinished();
        }
    }

    private void ProcessDecision(DialogDecision decision)
    {
        currentDecision = decision;

        AudioManager.Instance.PlayFX(decisionOptionClip);
        for(int i = 0; i < currentDecision.transform.childCount; ++i)
        {
            DialogDecisionOption answer = currentDecision.transform.GetChild(i).GetComponent<DialogDecisionOption>();
            if(answer)
            {
                if(answer.Condition.Test())
                {
                    GameObject newAnswer = Instantiate(answerPrefab, content.transform);
                    DialogAnswerBubble dialogAnswer = newAnswer.GetComponent<DialogAnswerBubble>();
                    dialogAnswer.SetContent(answer);
                    dialogAnswer.SetButtonEnabled(false);
                    dialogAnswer.OnSelected.AddListener(OnAnswerSelected);
                    OnContentAdded(newAnswer);
                    currentAnswers.Add(dialogAnswer);
                    StartTextAnimation(dialogAnswer, LocalizationManager.Instance.GetLocalizedString(answer.Text));
                }
            }
        }
    }

    private void ProcessRedirector(DialogRedirector redirector)
    {
        if(redirector.Target)
        {
            StartDialog(redirector.Target, redirector.Additive);
        }
    }

    private void ProcessSelector(DialogSelector selector)
    {
        // Test if the selector applies.
        if(selector.Condition.Test())
        {
            if(selector.transform.childCount > 0)
            {
                // The test was positive, so display all child elements of the selector.
                EnterElementContainer(selector);
            }
            else
            {
                // The selector is empty, so go to the next element.
                ProcessNextElement();
            }
        }
    }

    /**
     * Opens the shop or diary map for the selected option, if necessary.
     * @return True if a shop or diary was opened, false if no action was needed.
     */
    private bool HandleOption(DialogDecisionOption decisionOption)
    {
        switch (decisionOption.AnswerType)
        {
            case AnswerType.Quest:
            case AnswerType.Items:
            {
                if (decisionOption.shop)
                {
                    LevelInstance.Instance.OpenShop(decisionOption.shop);
                    return true;
                }
                break;
            }

            case AnswerType.Travel:
            {
                LevelInstance.Instance.OpenDiary(DiaryPageType.Map);
                return true;
            }
        }

        return false;
    }

    private void ProcessTriggerLastOption(DialogTriggerLastOption triggerLastOption)
    {
        if(!lastSelectedOption)
        {
            return;
        }

        if(!HandleOption(lastSelectedOption))
        {
            // The decision option either did not have an answertype selected that needs an action, or has invalid values,
            // so go ahead to the next element.
            if(!ProcessNextElement())
            {
                OnDialogFinished();
            }
        }
    }
    
    private void OnAnswerSelected(DialogAnswerBubble bubble)
    {
        // Cleanup. Set all buttons disabled in case animators get added later on.
        bubble.OnSelected.RemoveListener(OnAnswerSelected);
        foreach (DialogAnswerBubble dialogAnswerBubble in currentAnswers)
        {
            dialogAnswerBubble.SetButtonEnabled(false);
        }

        // Add the conditions to the list.
        AddConditions(currentDecision.SetConditions);
        AddConditions(bubble.Answer.SetConditions);

        // Save the y position of the first answer.
        DialogAnswerBubble firstBubble = currentAnswers[0];
        RectTransform firstTransform = firstBubble.GetComponent<RectTransform>();
        currentY = -firstTransform.anchoredPosition.y;

        // Destroy all answers except the chosen one.
        foreach(DialogAnswerBubble dialogAnswerBubble in currentAnswers)
        {
            if(dialogAnswerBubble != bubble)
            {
                Destroy(dialogAnswerBubble.gameObject);
            }
        }
        currentAnswers.Clear();
        currentDecision = null;
        lastSelectedOption = bubble.Answer;

        // Set the y position to the first answer.
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -currentY);
        currentY += bubbleTransform.rect.height;

        // Set the scrollrect content size.
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY);
        currentY += spacing;

        bool continueDialog = true;
        // Check if the decision option wants to trigger its action.
        if(bubble.Answer.autoTriggerAction)
        {
            continueDialog = !HandleOption(bubble.Answer);
        }

        if(continueDialog)
        {
            ContinueAfterLastOption();
        }
    }

    /**
     * Called after the overlay was closed, that was triggered of the selected option.
     */
    private void ContinueAfterLastOption()
    {
        if(!lastSelectedOption)
        {
            return;
        }

        // Display the next dialog after the answer (the childs of the answer).
        if (lastSelectedOption.transform.childCount > 0)
        {
            EnterElementContainer(lastSelectedOption);
        }
        else
        {
            OnDialogFinished();
        }
    }

    /**
     * Called when the back button was pressed during an overlay (when a shop or diary was opened during dialog).
     */
    public void OnOverlayClosed()
    {
        if(currentElement.Type == DialogElementType.TriggerLastOption)
        {
            // If the overlay was triggered because of TriggerLastOption, process the next element after this.
            if(!ProcessNextElement())
            {
                // There was no next element, so the dialog is finished.
                OnDialogFinished();
            }
        }
        else
        {
            // The overlay was triggered because the decision option had autoTriggerAction selected
            ContinueAfterLastOption();
        }
    }

    private void StartTextAnimation(IAnimatedText bubble, string text)
    {
        ElementAnimator animator = new TextElementAnimator(this, bubble, text, timeForCharacters);
        currentAnimators.Add(animator);
        animator.onFinished += OnAnimationFinished;
        animator.Start();
    }

    private void OnAnimationFinished(ElementAnimator animator)
    {
        animator.onFinished -= OnAnimationFinished;
        currentAnimators.Remove(animator);
        OnCurrentAnimatorsChanged();
    }

    private void OnCurrentAnimatorsChanged()
    {
        if (currentDecision != null && currentAnimators.Count == 0)
        {
            // Enable the buttons from the decision.
            foreach (DialogAnswerBubble dialogAnswerBubble in currentAnswers)
            {
                dialogAnswerBubble.SetButtonEnabled(true);
            }
        }
    }

    private void ClearContent()
    {
        for (int i = 0; i < content.transform.childCount; ++i)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
    }

    public void AddOnConditionsChanged(IEnumerable<string> conditions, OnConditionsChanged onConditionsChanged)
    {
        foreach(string condition in conditions)
        {
            AddOnConditionChanged(condition, onConditionsChanged);
        }
    }

    public void AddOnConditionChanged(string condition, OnConditionsChanged onConditionsChanged)
    {
        if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChanged changedEvent))
        {
            onConditionsChangedListeners[condition] = changedEvent + onConditionsChanged;
        }
        else
        {
            onConditionsChangedListeners.Add(condition, onConditionsChanged);
        }
    }

    /**
     * Adds a condition to the list.
     * @param global True if it should be added to the global list, false for the local one.
     */
    public void AddCondition(string condition, bool global = false)
    {
        if(string.IsNullOrWhiteSpace(condition))
        {
            return;
        }

        bool successful = false;
        if(global)
        {
            successful = NewGameManager.Instance.AddCondition(condition);
        }
        else
        {
            if(!conditions.Contains(condition))
            {
                conditions.Add(condition);
                successful = true;
            }
        }

        if(successful)
        {
            if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChanged onConditionsChanged))
            {
                onConditionsChanged.Invoke();
            }
        }
    }

    /**
     * Adds a condition to the list.
     */
    public void AddCondition(SetCondition condition)
    {
        AddCondition(condition.Condition, condition.IsGlobal);
    }

    /**
     * Adds multiple conditions to the list.
     */
    public void AddConditions(IEnumerable<SetCondition> conditions)
    {
        foreach(SetCondition condition in conditions)
        {
            AddCondition(condition);
        }
    }

    /**
     * Adds multiple conditions to the list.
     */
    public void AddConditions(IEnumerable<string> conditions, bool global)
    {
        foreach(string condition in conditions)
        {
            AddCondition(condition, global);
        }
    }

    /**
     * Removes a condition from the local and global list.
     */
    public void RemoveCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return;
        }

        bool successful = false;
        successful |= conditions.Remove(condition);
        successful |= NewGameManager.Instance.RemoveCondition(condition);
        if (successful)
        {
            if (onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChanged onConditionsChanged))
            {
                onConditionsChanged.Invoke();
            }
        }
    }

    /**
     * Removes conditions from the local and global list.
     */
    public void RemoveConditions(IEnumerable<string> conditions)
    {
        foreach(string condition in conditions)
        {
            RemoveCondition(condition);
        }
    }

    /**
     * Checks if one condition is met. Does not care whether it is met globally or locally.
     * If condition is empty, it is considered to be met.
     */
    public bool HasCondition(string condition)
    {
        if(string.IsNullOrWhiteSpace(condition))
        {
            return true;
        }

        if(conditions.Contains(condition))
        {
            return true;
        }

        return NewGameManager.Instance.HasCondition(condition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentAnimators.Count != 0)
        {
            foreach (ElementAnimator animator in currentAnimators)
            {
                animator.Finish();
            }
            currentAnimators.Clear();
            OnCurrentAnimatorsChanged();
            AudioManager.Instance.PlayCutTypewriter();
        }
        else if (currentElement != null)
        {
            ProcessNextElement();
        }
    }

    public static void LogValidateError(string message, GameObject go)
    {
        Debug.LogError($"({go.name}): {message}");
    }

    public static void ValidateSetConditions(IEnumerable<SetCondition> setConditions, GameObject go)
    {
        foreach(var condition in setConditions)
        {
            if(string.IsNullOrWhiteSpace(condition.Condition))
            {
                LogValidateError("Trying to set an empty condition", go);
            }
        }
    }

    public static void ValidateChildren(DialogElementType[] allowedTypes, GameObject go, bool requiresChildren = false)
    {
        if(requiresChildren && go.transform.childCount == 0)
        {
            LogValidateError("The dialog element must have children", go);
        }
        else if((allowedTypes == null || allowedTypes.Length == 0) && go.transform.childCount > 0)
        {
            LogValidateError("The dialog element may not contain children", go);
        }
        else
        {
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                DialogElement element = go.transform.GetChild(i).GetComponent<DialogElement>();
                if(element == null)
                {
                    LogValidateError($"Only dialog elements are allowed as children, not {go.transform.GetChild(i).name}", go);
                }
                else
                {
                    if(!allowedTypes.Contains(element.Type))
                    {
                        LogValidateError($"The element may not contain a {element.Type} dialog element as a child", go);
                    }
                }
            }
        }
    }
}
