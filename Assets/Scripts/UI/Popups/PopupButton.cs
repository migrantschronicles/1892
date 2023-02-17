using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PopupButtonSize
{
    Small,
    Medium,
    Large
}

public enum PopupButtonColor
{
    White,
    Gray,
    Green,
    Red,
    Brown
}

[System.Serializable]
public class PopupButtonStyle
{
    public Sprite small;
    public Sprite medium;
    public Sprite large;
    public Color foregroundColor = Color.white;
}

public class PopupButton : MonoBehaviour
{
    [SerializeField]
    private PopupButtonSize size = PopupButtonSize.Small;
    [SerializeField]
    private PopupButtonColor color = PopupButtonColor.Brown;
    [SerializeField]
    private PopupButtonStyle whiteStyle = new();
    [SerializeField]
    private PopupButtonStyle grayStyle = new();
    [SerializeField]
    private PopupButtonStyle greenStyle = new();
    [SerializeField]
    private PopupButtonStyle redStyle = new();
    [SerializeField]
    private PopupButtonStyle brownStyle = new();
    [SerializeField]
    private Image background;
    [SerializeField]
    private Text text;

    private void Awake()
    {
        UpdateStyle();
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

        UpdateStyle();
    }
#endif

    private void UpdateStyle()
    {
        PopupButtonStyle style = null;
        switch(color)
        {
            case PopupButtonColor.White: style = whiteStyle; break;
            case PopupButtonColor.Green: style = greenStyle; break;
            case PopupButtonColor.Red: style = redStyle; break;
            case PopupButtonColor.Brown: style = brownStyle; break;
            case PopupButtonColor.Gray: style = grayStyle; break;
        }

        Sprite backgroundSprite = null;
        switch(size)
        {
            case PopupButtonSize.Small: backgroundSprite = style.small; break;
            case PopupButtonSize.Medium: backgroundSprite = style.medium; break;
            case PopupButtonSize.Large: backgroundSprite = style.large; break;
        }

        background.sprite = backgroundSprite;
        text.color = style.foregroundColor;
        GetComponent<RectTransform>().sizeDelta = backgroundSprite.rect.size;
    }
}
