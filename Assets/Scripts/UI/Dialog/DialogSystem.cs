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
    private float currentY = 0;
    private Coroutine textAnimationRoutine;

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
            if(textAnimationRoutine != null)
            {
                FinishTextAnimation();
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
            if(dialog.TestCondition())
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
        ProcessNextItem();
    }

    private void Reset()
    {
        if(textAnimationRoutine != null)
        {
            StopCoroutine(textAnimationRoutine);
            textAnimationRoutine = null;
        }

        currentDialog = null;
        currentItem = null;
        currentBubble = null;
    }

    private void ProcessNextItem()
    {
        if(currentItem)
        {
            int nextIndex = currentItem.transform.GetSiblingIndex() + 1;
            if(nextIndex < currentItem.transform.parent.childCount)
            {
                currentItem = currentItem.transform.parent.GetChild(nextIndex).GetComponent<DialogItem>();
            }
            else
            {
                ///@todo 
                return;
            }
        }
        else
        {
            if(currentDialog.transform.childCount == 0)
            {
                return;
            }

            currentItem = currentDialog.transform.GetChild(0).GetComponent<DialogItem>();
        }

        ProcessItem(currentItem);
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
        }
    }

    private void ProcessLine(DialogLine line)
    {
        GameObject newLine = Instantiate(linePrefab, content.transform);
        currentBubble = newLine.GetComponent<DialogBubble>();
        currentBubble.SetContent(line.Text, line.IsLeft);
        RectTransform rectTransform = newLine.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -currentY);
        currentY += rectTransform.rect.height;
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY);
        currentY += spacing;
        textAnimationRoutine = StartCoroutine(AnimateCurrentLine(line.Text));
    }

    private IEnumerator AnimateCurrentLine(string value)
    {
        for(int i = 1; i <= value.Length; ++i)
        {
            currentBubble.SetText(value.Substring(0, i));
            yield return new WaitForSeconds(timeForCharacters);
        }

        textAnimationRoutine = null;
    }

    private void FinishTextAnimation()
    { 
        if(textAnimationRoutine != null)
        {
            StopCoroutine(textAnimationRoutine);
            textAnimationRoutine = null;
            DialogLine line = (DialogLine)currentItem;
            currentBubble.SetText(line.Text);
        }
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

        if(condition.Contains(condition))
        {
            return true;
        }

        return globalConditions.Contains(condition);
    }
}
