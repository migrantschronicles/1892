using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject answerPrefab;
    [SerializeField]
    private float spacing = 30;
    [SerializeField]
    private float timeForCharacters = 0.1f;

    private GameObject content;
    private List<string> conditions = new List<string>();
    ///@todo Should be in the game manager.
    private static List<string> globalConditions = new List<string>();

    private Dialog currentDialog;
    private DialogItem currentItem;
    private DialogBubble currentBubble;
    private DialogDecision currentDecision;
    private float currentY = 0;
    private List<DialogAnswerBubble> currentAnswers = new List<DialogAnswerBubble>();
    private List<DialogAnimator> currentAnimators = new List<DialogAnimator>();

    public float TimeForCharacters
    {
        get
        {
            return timeForCharacters;
        }
    }

    private void Awake()
    {
        Instance = this;
        ScrollRect scrollView = GetComponent<ScrollRect>();
        content = scrollView.content.gameObject;
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
            }
            else if(currentItem != null)
            {
                ProcessNextItem();
            }
        }
    }

    public void StartDialog(GameObject parent)
    {
        StartDialog(parent, false);
    }

    public void StartDialog(GameObject parent, bool additive)
    {
        for(int i = 0; i < parent.transform.childCount; ++i)
        {
            Dialog dialog = parent.transform.GetChild(i).GetComponent<Dialog>();
            if(dialog.Condition.Test())
            {
                StartDialog(dialog, additive);
                return;
            }
        }
    }

    public void StartDialog(Dialog dialog)
    {
        StartDialog(dialog, false);
    }

    public void StartDialog(Dialog dialog, bool additive)
    {
        gameObject.SetActive(true);
        if (!additive)
        {
            ClearContent();
            currentY = 0;
        }

        Reset();
        currentDialog = dialog;
        EnterContainerItem(currentDialog);
    }

    private void Reset()
    {
        foreach (DialogAnimator animator in currentAnimators)
        {
            animator.Finish();
        }
        currentAnimators.Clear();

        currentDialog = null;
        currentItem = null;
        currentBubble = null;
        currentDecision = null;
        currentAnswers.Clear();
    }

    private void EnterContainerItem(DialogItem parent)
    {
        if(parent.transform.childCount == 0)
        {
            // There are no childs to process
            currentItem = null;
            return;
        }

        currentItem = parent.transform.GetChild(0).GetComponent<DialogItem>();
        ProcessItem(currentItem);
    }

    private void ProcessNextItem()
    {
        if(currentItem)
        {
            int nextIndex = currentItem.transform.GetSiblingIndex() + 1;
            if(nextIndex < currentItem.transform.parent.childCount)
            {
                currentItem = currentItem.transform.parent.GetChild(nextIndex).GetComponent<DialogItem>();
                ProcessItem(currentItem);
            }
            else
            {
                ///@todo 
                return;
            }
        }
    }

    private void ProcessItem(DialogItem item)
    {
        switch(item.Type)
        {
            case DialogItemType.Line:
            {
                ProcessLine((DialogLine)item);
                break;
            }

            case DialogItemType.Decision:
            {
                ProcessDecision((DialogDecision)item);
                break;
            }
        }
    }

    private void OnContentAdded(GameObject newContent)
    {
        RectTransform rectTransform = newContent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -currentY);
        currentY += rectTransform.rect.height;
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY);
        currentY += spacing;
    }

    private void ProcessLine(DialogLine line)
    {
        GameObject newLine = Instantiate(linePrefab, content.transform);
        currentBubble = newLine.GetComponent<DialogBubble>();
        currentBubble.SetContent(line);
        OnContentAdded(newLine);
        StartTextAnimation(currentBubble, line.Text);
    }

    private void ProcessDecision(DialogDecision decision)
    {
        currentDecision = decision;

        for(int i = 0; i < currentDecision.transform.childCount; ++i)
        {
            DialogAnswer answer = currentDecision.transform.GetChild(i).GetComponent<DialogAnswer>();
            if(answer)
            {
                if(answer.Condition.Test())
                {
                    GameObject newAnswer = Instantiate(answerPrefab, content.transform);
                    DialogAnswerBubble dialogAnswer = newAnswer.GetComponent<DialogAnswerBubble>();
                    dialogAnswer.SetContent(answer);
                    dialogAnswer.OnSelected.AddListener(OnAnswerSelected);
                    OnContentAdded(newAnswer);
                    currentAnswers.Add(dialogAnswer);
                    StartTextAnimation(dialogAnswer, answer.Text);
                }
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
        SetCondition(currentDecision.SetCondition, currentDecision.IsGlobal);
        SetCondition(bubble.Answer.SetCondition, bubble.Answer.IsGlobal);

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
        EnterContainerItem(bubble.Answer);
    }

    private void StartTextAnimation(IDialogBubble bubble, string text)
    {
        DialogAnimator animator = new DialogTextAnimator(this, bubble, text, timeForCharacters);
        currentAnimators.Add(animator);
        animator.OnFinished += StopAnimation;
        animator.Start();
    }

    private void StopAnimation(DialogAnimator animator)
    {
        animator.OnFinished -= StopAnimation;
        currentAnimators.Remove(animator);
    }

    private void ClearContent()
    {
        for (int i = 0; i < content.transform.childCount; ++i)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
    }

    public void SetCondition(string condition, bool global = false)
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
