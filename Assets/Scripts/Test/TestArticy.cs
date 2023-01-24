using Articy.TheMigrantsChronicles;
using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ArticyFlowPlayer))]
public class TestArticy : MonoBehaviour, IScriptMethodProvider, IPointerClickHandler
{
    public GameObject fragmentPrefab;
    public GameObject optionPrefab;
    public ArticyRef startObject;
    public string mainTechnicalName;
    [SerializeField, Tooltip("The vertical space between each bubble")]
    private float spacing = 30;
    [SerializeField]
    private float paddingBottom = 8;

    private ArticyFlowPlayer flowPlayer;
    private GameObject content;
    private float currentY = 0;

    public bool IsCalledInForecast { get; set; }

    public static TestArticy Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        flowPlayer = GetComponent<ArticyFlowPlayer>();
        // set the default method provider for script methods, so that we needn't pass it as a parameter when calling script methods manually.
        // look into region "script methods" at the end of this class for more information.
        ArticyDatabase.DefaultMethodProvider = this;

        content = GetComponent<ScrollRect>().content.gameObject;
    }

    private void Start()
    {
        ContinueFlow(startObject.GetObject());
    }

    /// <summary>
    /// This method is used to trigger different behaviours depending on the result or outcome of an interaction by the user.
    /// </summary>
    public void ContinueFlow(IArticyObject aObject)
    {
        if(aObject != null)
        {
            // and we assign the object as the start node
            flowPlayer.StartOn = aObject;
        }
    }

    /// <summary>
    /// This is one of the important callbacks from the ArticyFlowPlayer, and will notify us about pausing on any flow object.
    /// It will make sure that the paused object is displayed in our dialog ui, by extracting its text, potential speaker etc.
    /// </summary>
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        // if the flow player paused on a dialog, we immediately continue, usually getting to the first dialogue fragment inside the dialogue
        // makes it more convenient to set the startOn to a dialogue
        if (aObject is IDialogue)
        {
            flowPlayer.Play();
            return;
        }

        // Create new speech bubble
        GameObject newBubbleGO = Instantiate(fragmentPrefab, content.transform);
        DialogBubbleArticy bubble = newBubbleGO.GetComponent<DialogBubbleArticy>();
        bubble.AssignFlowObject(flowPlayer, aObject);
        ///@todo Loca caretaker runs next frame, so this uses the default height.
        OnContentAdded(newBubbleGO);
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
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, currentY + paddingBottom);
        // Add the spacing to the current y value, so the next new content will be placed slightly below.
        currentY += spacing;

        // Set the position of the scroll rect to scroll to the new content.
        Canvas.ForceUpdateCanvases();
        float newY = ((Vector2)transform.InverseTransformPoint(contentTransform.position) -
            (Vector2)transform.InverseTransformPoint(newContent.transform.position)).y;
        contentTransform.anchoredPosition = new Vector2(contentTransform.anchoredPosition.x, newY);
    }

    private IList<Branch> TEST_NextBranches;

    /// <summary>
    /// This is the other important callback from the ArticyFlowPlayer, and is called everytime the flow player has new branches
    /// for us. We use that to update the list of buttons in our dialog interface.
    /// </summary>
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        TEST_NextBranches = aBranches;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(TEST_NextBranches.Count == 1)
        {
            Branch nextBranch = TEST_NextBranches[0];
            TEST_NextBranches = null;
            ContinueFlow(nextBranch.Target as IArticyObject);
        }
    }
}
