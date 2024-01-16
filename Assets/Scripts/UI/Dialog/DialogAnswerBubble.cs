using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum AnswerType
{
    Talking,
    Quest,
    Travel,
    Items,
    MoneyExchange
}

public class DialogAnswerBubble : MonoBehaviour, IAnimatedText
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
    private Image icon;
    [SerializeField]
    private Button button;
    [SerializeField]
    private bool isSelected;
    [SerializeField]
    private bool isEnabled = true;
    [SerializeField]
    private Color defaultBackgroundColor = Color.white;
    [SerializeField]
    private Color defaultDiamondColor = new Color(0.9215686275f, 0.9215686275f, 0.9215686275f);
    [SerializeField]
    private Color defaultForegroundColor = Color.black;
    [SerializeField]
    private Color selectedBackgroundColor = new Color(0.6f, 0.6509803922f, 0.4627450980f);
    [SerializeField]
    private Color selectedDiamondColor = new Color(0.6745098039f, 0.7137254902f, 0.5647058824f);
    [SerializeField]
    private Color selectedForegroundColor = Color.white;
    [SerializeField]
    private Color disabledBackgroundColor = Color.grey;
    [SerializeField]
    private Color disabledDiamondColor = new Color(0.4607843138f, 0.4607843138f, 0.4607843138f);
    [SerializeField]
    private Color disabledForegroundColor = Color.white;
    [SerializeField]
    private Sprite talkingIcon;
    [SerializeField]
    private Sprite questIcon;
    [SerializeField]
    private Sprite travelIcon;
    [SerializeField]
    private Sprite itemsIcon;
    [SerializeField]
    private Sprite moneyExchangeIcon;

    private DropShadow[] shadows;
    private AnswerType answerType = AnswerType.Talking;
    
    public Branch Branch { get; private set; }

    public delegate void OnSelectedEvent(DialogAnswerBubble bubble);
    public event OnSelectedEvent OnSelected;

    private void Awake()
    {
        locaCaretaker.localizedTextAssignmentMethod.AddListener(OnLocalizedTextChanged);
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if(!isSelected && DialogSystem.Instance.IsCurrentBranch(this))
        {
            isSelected = true;
            UpdateColors();
            DialogSystem.Instance.UnregisterAnimator(this);
            OnSelected.Invoke(this);
        }
    }

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
        UpdatePosition();
        UpdateIcon();
        UpdateButton();
        UpdateShadows();
    }
#endif

    private void UpdateButton()
    {
        button.enabled = isEnabled;
    }

    private void UpdateColors()
    {
        background.color = isSelected ? selectedBackgroundColor : (isEnabled ? defaultBackgroundColor : disabledBackgroundColor);
        diamond.color = isSelected ? selectedDiamondColor : (isEnabled ? defaultDiamondColor : disabledDiamondColor);
        text.color = isSelected ? selectedForegroundColor : (isEnabled ? defaultForegroundColor : disabledForegroundColor);
    }

    private void UpdatePosition()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform diamondTransform = diamond.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-diamondTransform.sizeDelta.x / 2, rectTransform.anchoredPosition.y);
    }

    private void UpdateIcon()
    {
        Sprite sprite = null;
        switch(answerType)
        {
            case AnswerType.Talking: sprite = talkingIcon; break;
            case AnswerType.Quest: sprite = questIcon; break;
            case AnswerType.Travel: sprite = travelIcon; break;
            case AnswerType.Items: sprite = itemsIcon; break;
            case AnswerType.MoneyExchange: sprite = moneyExchangeIcon; break;
        }

        icon.sprite = sprite;
    }

    private void UpdateShadows()
    {
        if(shadows == null || shadows.Length == 0)
        {
            shadows = GetComponentsInChildren<DropShadow>();
        }

        foreach (DropShadow shadow in shadows)
        {
            shadow.enabled = isSelected || isEnabled;
        }
    }

    public void AssignBranch(Branch branch, string overrideText = null)
    {
        Branch = branch;

        if(overrideText != null)
        {
            locaCaretaker.locaKey = overrideText;
        }
        else
        {
            var modelWithMenuText = branch.Target as IObjectWithLocalizableMenuText;
            if(modelWithMenuText != null)
            {
                locaCaretaker.locaKey = modelWithMenuText.LocaKey_MenuText;
            }
            else
            {
                locaCaretaker.locaKey = "...";
            }
        }

        UpdateColors();
        UpdatePosition();
        UpdateIcon();
        UpdateButton();
        UpdateShadows();
    }

    private void OnLocalizedTextChanged(Component targetComponent, string localizedText)
    {
        string resolvedText = DialogSystem.Instance.ResolveDialogLine(localizedText);
        text.text = resolvedText;

        if(DialogSystem.Instance.IsCurrentBranch(this))
        {
            // Check that this decision option is a current one and not an old one.
            DialogSystem.Instance.RegisterAnimator(this, resolvedText);
        }
    }

    public void SetText(string value)
    {
        text.text = DialogSystem.Instance.ResolveDialogLine(value);
    }

    public void SetButtonEnabled(bool enabled = true)
    {
        button.enabled = isEnabled && enabled;
    }
}
