using Articy.Unity;
using Articy.Unity.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogChat : MonoBehaviour
{
    class Entry
    {
        public GameObject bubble;
    }

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject answerPrefab;
    [SerializeField, Tooltip("The vertical space between each bubble")]
    private float spacing = 30;
    [SerializeField, Tooltip("How much space on the bottom should be left for the shadow to display")]
    private float paddingBottom = 8;

    private List<Entry> entries = new List<Entry>();
    private RectTransform rectTransform;
    private List<DialogAnswerBubble> currentAnswers = new List<DialogAnswerBubble>();
    private IFlowObject pausedOn;
    private IList<Branch> availableBranches;

    public bool IsWaitingForDecision { get { return currentAnswers.Count > 0; } }
    public float Height { get { return rectTransform.sizeDelta.y; } }

    public delegate void OnHeightChangedEvent(float height);
    public event OnHeightChangedEvent OnHeightChanged;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnFlowPlayerPaused(IFlowObject flowObject)
    {
        pausedOn = flowObject;

        GameObject bubbleGO = Instantiate(linePrefab, transform);
        AddToContent(bubbleGO);
        entries.Add(new Entry { bubble = bubbleGO });

        DialogBubble bubble = bubbleGO.GetComponent<DialogBubble>();
        bubble.OnHeightChanged += OnBubbleHeightChanged;
        bubble.AssignFlowObject(flowObject);
    }

    public void OnBranchesUpdated(IList<Branch> branches)
    {
        availableBranches = branches;
    }

    public void OnClosing()
    {
    }

    private void OnBubbleHeightChanged(DialogBubble bubble, float oldHeight, float newHeight)
    {
        ///@todo Only accounts if it's the last bubble.
        if(bubble.gameObject != entries[entries.Count - 1].bubble)
        {
            Debug.LogError($"{bubble.name} is not the last added bubble, but changed it's height");
        }

        float adjustment = newHeight - oldHeight;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + adjustment);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    public void OnPointerClick()
    {
        if(availableBranches == null)
        {
            return;
        }

        bool isDialogFinished = true;
        foreach(var branch in availableBranches)
        {
            if(branch.Target is IDialogueFragment)
            {
                isDialogFinished = false;
                break;
            }
        }

        if(!isDialogFinished)
        {
            if (availableBranches.Count == 1)
            {
                // A linear dialog flow, so go to the next line and create a bubble.
                Branch targetBranch = availableBranches[0];
                availableBranches = null;
                DialogSystem.Instance.FlowPlayer.StartOn = targetBranch.Target as IArticyObject;
            }
            else
            {
                // Multiple branches, so it's a decision.
                if (!IsWaitingForDecision)
                {
                    foreach (var branch in availableBranches)
                    {
                        // we filter those out that are not valid
                        if (!branch.IsValid)
                        {
                            continue;
                        }

                        GameObject bubbleGO = Instantiate(answerPrefab, transform);
                        AddToContent(bubbleGO);
                        entries.Add(new Entry { bubble = bubbleGO });

                        DialogAnswerBubble bubble = bubbleGO.GetComponent<DialogAnswerBubble>();
                        bubble.AssignBranch(branch);
                        bubble.OnSelected += OnDecisionTaken;
                        currentAnswers.Add(bubble);
                    }
                }
            }
        }
    }

    private void AddToContent(GameObject bubble)
    {
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        float currentHeight = Mathf.Max(0, rectTransform.sizeDelta.y - paddingBottom);
        float newY = currentHeight + (entries.Count == 0 ? 0 : spacing);
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -newY);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newY + bubbleTransform.sizeDelta.y + paddingBottom);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    private void OnDecisionTaken(DialogAnswerBubble bubble)
    {
        // Calculate the height reduction after every decision option was removed.
        float adjustment = 0.0f;
        bool containsBubble = false;
        foreach(var answer in currentAnswers)
        {
            RectTransform answerTransform = answer.GetComponent<RectTransform>();
            adjustment += answerTransform.sizeDelta.y + spacing;
            if(answer != bubble)
            {
                answer.transform.SetParent(null, false);
                Destroy(answer);
            }
            else
            {
                containsBubble = true;
            }
        }

        if(!containsBubble)
        {
            // Was an old bubble that was not selected.
            return;
        }

        // Reposition the selected bubble and adjust the chat height.
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        float newY = rectTransform.sizeDelta.y - adjustment + spacing - paddingBottom;
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -newY);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newY + bubbleTransform.sizeDelta.y + paddingBottom);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);

        currentAnswers.Clear();
        DialogSystem.Instance.FlowPlayer.StartOn = bubble.Branch.Target as IArticyObject;
    }

    public bool IsCurrentBranch(DialogAnswerBubble bubble)
    {
        return currentAnswers.Contains(bubble);
    }
}
