using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
 * You can also set the conditions that should be added if this specific option is chosen.
 * You can also add conditions that must be met so that the option is displayed in the first place.
 * This can be useful if you do not have an option anymore because of some action on another level.
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
public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject answerPrefab;
    [SerializeField]
    private Button closeButton;
    [SerializeField, Tooltip("The vertical space between each bubble")]
    private float spacing = 30;
    [SerializeField, Tooltip("The time for each character in a text animation")]
    private float timeForCharacters = 0.1f;

    private GameObject content;
    private List<string> conditions = new List<string>();
    ///@todo Should be in the game manager.
    private static List<string> globalConditions = new List<string>();

    private Dialog currentDialog;
    private DialogElement currentElement;
    private DialogBubble currentBubble;
    private DialogDecision currentDecision;
    private float currentY = 0;
    private List<DialogAnswerBubble> currentAnswers = new List<DialogAnswerBubble>();
    private List<DialogAnimator> currentAnimators = new List<DialogAnimator>();

    private void Awake()
    {
        Instance = this;
        ScrollRect scrollView = GetComponent<ScrollRect>();
        content = scrollView.content.gameObject;
    }

    private void Start()
    {
        closeButton?.onClick.AddListener(OnClose);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(currentAnimators.Count != 0)
            {
                foreach(DialogAnimator animator in currentAnimators)
                {
                    animator.Finish();
                }
                currentAnimators.Clear();
                OnCurrentAnimatorsChanged();
            }
            else if(currentElement != null)
            {
                ProcessNextElement();
            }
        }
    }

    private void Activate()
    {
        gameObject.SetActive(true);
        closeButton?.gameObject.SetActive(true);
    }

    private void OnClose()
    {
        StopAllCoroutines();
        currentAnimators.Clear();
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

    private void ResetState()
    {
        foreach (DialogAnimator animator in currentAnimators)
        {
            animator.Finish();
        }
        currentAnimators.Clear();

        currentDialog = null;
        currentElement = null;
        currentBubble = null;
        currentDecision = null;
        currentAnswers.Clear();
    }

    private void EnterElementContainer(DialogElement parent)
    {
        if(parent.transform.childCount == 0)
        {
            // There are no childs to process
            currentElement = null;
            return;
        }

        currentElement = parent.transform.GetChild(0).GetComponent<DialogElement>();
        ProcessElement(currentElement);
    }

    private void ProcessNextElement()
    {
        if(currentElement)
        {
            int nextIndex = currentElement.transform.GetSiblingIndex() + 1;
            if(nextIndex < currentElement.transform.parent.childCount)
            {
                // Process the next sibling element.
                currentElement = currentElement.transform.parent.GetChild(nextIndex).GetComponent<DialogElement>();
                ProcessElement(currentElement);
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
                    }
                    else
                    {
                        // The parent selector has no next elements, so try the parent of the parent selector.
                        parentSelector = parentSelector.transform.parent.GetComponent<DialogSelector>();
                    }
                }
            }
        }
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
        StartTextAnimation(currentBubble, line.Text.GetLocalizedString());
    }

    private void ProcessDecision(DialogDecision decision)
    {
        currentDecision = decision;

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
                    StartTextAnimation(dialogAnswer, answer.Text.GetLocalizedString());
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

        // Set the y position to the first answer.
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -currentY);
        currentY += bubbleTransform.rect.height;

        // Set the scrollrect content size.
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY);
        currentY += spacing;

        // Display the next dialog after the answer (the childs of the answer).
        EnterElementContainer(bubble.Answer);
    }

    private void StartTextAnimation(IDialogBubble bubble, string text)
    {
        DialogAnimator animator = new DialogTextAnimator(this, bubble, text, timeForCharacters);
        currentAnimators.Add(animator);
        animator.OnFinished += OnAnimationFinished;
        animator.Start();
    }

    private void OnAnimationFinished(DialogAnimator animator)
    {
        animator.OnFinished -= OnAnimationFinished;
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

        if(global)
        {
            if(!globalConditions.Contains(condition))
            {
                globalConditions.Add(condition);
            }
        }
        else
        {
            if(!conditions.Contains(condition))
            {
                conditions.Add(condition);
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

        return globalConditions.Contains(condition);
    }
}
