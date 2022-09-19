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
    private float AnchoredPositionX = 50;

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
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(AnchoredPositionX, rectTransform.anchoredPosition.y);

        RectTransform diamond = Diamond.GetComponent<RectTransform>();
        diamond.anchorMin = new Vector2(0, 0);
        diamond.anchorMax = new Vector2(0, 0);

        RectTransform background = Background.GetComponent<RectTransform>();
        background.localScale = new Vector3(-1, 1, 1);
    }

    private void SetRight()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-AnchoredPositionX, rectTransform.anchoredPosition.y);

        RectTransform diamond = Diamond.GetComponent<RectTransform>();
        diamond.anchorMin = new Vector2(1, 0);
        diamond.anchorMax = new Vector2(1, 0);

        RectTransform background = Background.GetComponent<RectTransform>();
        background.localScale = new Vector3(1, 1, 1);
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
