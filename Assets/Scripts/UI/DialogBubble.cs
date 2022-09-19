using UnityEngine;
using UnityEngine.UI;

public class DialogBubble : MonoBehaviour
{
    [SerializeField]
    private Image Background;
    [SerializeField]
    private Image Diamond;
    [SerializeField]
    private Text Text;
    [SerializeField]
    private bool IsLeft = false;
    [SerializeField]
    private Color PlayerBackgroundColor = new Color(0.2705882353f, 0.1725490196f, 0.1333333333f);
    [SerializeField]
    private Color PlayerDiamondColor = new Color(0.3764705882f, 0.2627450980f, 0.1843137255f);
    [SerializeField]
    private Color NPCBackgroundColor = new Color(0.2666666667f, 0.2666666667f, 0.2666666667f);
    [SerializeField]
    private Color NPCDiamondColor = new Color(0.3607843137f, 0.3607843137f, 0.3607843137f);

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

        if (IsLeft)
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
        RectTransform diamond = Diamond.GetComponent<RectTransform>();
        RectTransform background = Background.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(diamond.sizeDelta.x / 2, rectTransform.anchoredPosition.y);

        diamond.anchorMin = new Vector2(0, 0);
        diamond.anchorMax = new Vector2(0, 0);

        background.localScale = new Vector3(-1, 1, 1);

        Background.color = NPCBackgroundColor;
        Diamond.color = NPCDiamondColor;
    }

    private void SetRight()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform diamond = Diamond.GetComponent<RectTransform>();
        RectTransform background = Background.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-diamond.sizeDelta.x / 2, rectTransform.anchoredPosition.y);

        diamond.anchorMin = new Vector2(1, 0);
        diamond.anchorMax = new Vector2(1, 0);

        background.localScale = new Vector3(1, 1, 1);

        Background.color = PlayerBackgroundColor;
        Diamond.color = PlayerDiamondColor;
    }

    private void UpdateHeight()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform textTransform = Text.GetComponent<RectTransform>();
        float padding = textTransform.offsetMin.x;
        float newHeight = Text.preferredHeight + padding * 2;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
    }
}
