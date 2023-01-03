using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogBubble : MonoBehaviour, IAnimatedText
{
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

    public DialogLine Line { get; private set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if(this == null)
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

    public void SetContent(DialogLine line, string content)
    {
        Line = line;
        text.text = content;
        isLeft = line.IsLeft;

        UpdateHeight();
        if (isLeft)
        {
            SetLeft();
        }
        else
        {
            SetRight();
        }
    }

    /**
     * Does not update the height of the bubble.
     */
    public void SetText(string value)
    {
        text.text = value;
    }
}
