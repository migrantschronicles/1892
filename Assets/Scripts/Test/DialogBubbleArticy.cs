using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBubbleArticy : MonoBehaviour
{
    [SerializeField]
    private ArticyLocaCaretaker locaCaretaker;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image diamond;
    [SerializeField]
    private Text text;
    [SerializeField]
    private bool isLeft = false;
    [SerializeField]
    private Color playerBackgroundColor = new Color(0.2705882353f, 0.1725490196f, 0.1333333333f);
    [SerializeField]
    private Color playerDiamondColor = new Color(0.3764705882f, 0.2627450980f, 0.1843137255f);
    [SerializeField]
    private Color playerForegroundColor = Color.white;
    [SerializeField]
    private Color npcBackgroundColor = new Color(0.2666666667f, 0.2666666667f, 0.2666666667f);
    [SerializeField]
    private Color npcDiamondColor = new Color(0.3607843137f, 0.3607843137f, 0.3607843137f);
    [SerializeField]
    private Color npcForegroundColor = Color.white;

    private IFlowObject flowObject;
    private ArticyFlowPlayer processor;

    private void Awake()
    {
        locaCaretaker.localizedTextAssignmentMethod.AddListener(AssignText);
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

        if (isLeft)
        {
            SetLeft();
        }
        else
        {
            SetRight();
        }

        UpdateHeight();
    }
#endif

    private void SetLeft()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform diamondTransform = diamond.GetComponent<RectTransform>();
        RectTransform backgroundTransform = background.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(diamondTransform.sizeDelta.x / 2, rectTransform.anchoredPosition.y);

        diamondTransform.anchorMin = new Vector2(0, 0);
        diamondTransform.anchorMax = new Vector2(0, 0);

        backgroundTransform.localScale = new Vector3(-1, 1, 1);

        background.color = npcBackgroundColor;
        diamond.color = npcDiamondColor;
        text.color = npcForegroundColor;

        DropShadow shadow = background.GetComponent<DropShadow>();
        shadow.EffectDistance = new Vector2(-Mathf.Abs(shadow.EffectDistance.x), shadow.EffectDistance.y);
    }

    private void SetRight()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform diamondTransform = diamond.GetComponent<RectTransform>();
        RectTransform backgroundTransform = background.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-diamondTransform.sizeDelta.x / 2, rectTransform.anchoredPosition.y);

        diamondTransform.anchorMin = new Vector2(1, 0);
        diamondTransform.anchorMax = new Vector2(1, 0);

        backgroundTransform.localScale = new Vector3(1, 1, 1);

        background.color = playerBackgroundColor;
        diamond.color = playerDiamondColor;
        text.color = playerForegroundColor;

        DropShadow shadow = background.GetComponent<DropShadow>();
        shadow.EffectDistance = new Vector2(Mathf.Abs(shadow.EffectDistance.x), shadow.EffectDistance.y);
    }

    private void UpdateHeight()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform textTransform = text.GetComponent<RectTransform>();
        float padding = textTransform.offsetMin.x;
        float newHeight = text.preferredHeight + padding * 2;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
    }

    public void AssignFlowObject(ArticyFlowPlayer aProcessor, IFlowObject aObject, string aOverrideText = null)
    {
        processor = aProcessor;
        flowObject = aObject;

        // the caller could set a text that he wants to use, otherwise we build it using the information we find inside the branch
        if(aOverrideText != null)
        {
            locaCaretaker.locaKey = aOverrideText;
        }
        else
        {
            // here we extract any Text from the paused object.
            // In most cases this is the spoken text of a dialogue fragment
            var modelWithText = aObject as IObjectWithLocalizableText;
            if(modelWithText != null)
            {
                // but we are not taking Text directly and assigning it to the ui control
                // we take the LocaKey and assign it to our caretaker. The caretaker will localize it
                // using the current language and will set it to our text control. The caretaker will also make sure that the text
                // is updated and localized again if we change the language while it is currently displayed to the screen.
                locaCaretaker.locaKey = modelWithText.LocaKey_Text;
            }
            else
            {
                locaCaretaker.locaKey = "...";
            }
        }

        // finally we update the speaker image in our ui, by checking if the paused object has a speaker
        var dlgSpeaker = aObject as IObjectWithSpeaker;
        if (dlgSpeaker != null)
        {
            // getting the speaker object
            var speaker = dlgSpeaker.Speaker;
            if (speaker != null)
            {
                if(speaker.TechnicalName == TestArticy.Instance.mainTechnicalName)
                {
                    SetRight();
                }
                else
                {
                    SetLeft();
                }
            }
        }
    }

    private void AssignText(Component aTargetComponent, string aLocalizedText)
    {
        var text = aTargetComponent as Text;
        text.text = aLocalizedText;

        UpdateHeight();
    }
}
