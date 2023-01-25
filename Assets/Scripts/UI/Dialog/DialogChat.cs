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

    private float currentY = 0;
    private List<Entry> entries = new List<Entry>();
    private RectTransform rectTransform;
    private IList<Branch> nextBranches;

    public delegate void OnHeightChangedEvent(float height);
    public event OnHeightChangedEvent OnHeightChanged;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnFlowPlayerPaused(IFlowObject flowObject)
    {
        GameObject bubbleGO = Instantiate(linePrefab, transform);

        RectTransform bubbleTransform = bubbleGO.GetComponent<RectTransform>();
        float currentHeight = rectTransform.sizeDelta.y;
        float newY = currentHeight + (entries.Count == 0 ? 0 : spacing);
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -newY);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newY + bubbleTransform.sizeDelta.y + paddingBottom);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);

        entries.Add(new Entry { bubble = bubbleGO });
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);

        DialogBubble bubble = bubbleGO.GetComponent<DialogBubble>();
        bubble.OnHeightChanged += OnBubbleHeightChanged;
        bubble.AssignFlowObject(flowObject);
    }

    public void OnBranchesUpdated(IList<Branch> branches)
    {
        nextBranches = branches;
    }

    private void OnBubbleHeightChanged(DialogBubble bubble, float oldHeight, float newHeight)
    {
        float adjustment = newHeight - oldHeight;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + adjustment);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    public void OnPointerClick()
    {
        if(nextBranches == null)
        {
            return;
        }

        if(nextBranches.Count == 1)
        {
            Branch targetBranch = nextBranches[0];
            nextBranches = null;
            DialogSystem.Instance.FlowPlayer.StartOn = targetBranch.Target as IArticyObject;
        }
    }
}
