using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AnswerType
{
    Talking,
    Quest,
    Travel,
    Items
}

public class DialogAnswer : MonoBehaviour
{
    [SerializeField]
    private AnswerType answerType;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image diamond;
    [SerializeField]
    private Text text;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private bool isSelected;
    [SerializeField]
    private Color defaultBackgroundColor = Color.white;
    [SerializeField]
    private Color defaultDiamondColor = new Color(0.9215686275f, 0.9215686275f, 0.9215686275f);
    [SerializeField]
    private Color selectedBackgroundColor = new Color(0.6f, 0.6509803922f, 0.4627450980f);
    [SerializeField]
    private Color selectedDiamondColor = new Color(0.6745098039f, 0.7137254902f, 0.5647058824f);

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

        UpdateColors();
    }
#endif

    private void UpdateColors()
    {
        background.color = isSelected ? selectedBackgroundColor : defaultBackgroundColor;
        diamond.color = isSelected ? selectedDiamondColor : defaultDiamondColor;
    }
}
