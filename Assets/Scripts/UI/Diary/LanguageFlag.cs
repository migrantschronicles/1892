using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageFlag : MonoBehaviour
{
    [SerializeField]
    private Language language = Language.English;
    [SerializeField]
    private Button button;
    [SerializeField]
    private Sprite englishFlag;
    [SerializeField]
    private Sprite luxFlag;

    private Material material;

    private void Start()
    {
        Image image = (Image)button.targetGraphic;
        material = Instantiate(image.material);
        image.material = material;
        UpdateLanguage();
        button.onClick.AddListener(OnClick);
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    private void OnEnable()
    {
        OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
    }

    private void OnDestroy()
    {
        if(LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
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

        UpdateLanguage();
    }
#endif

    private void UpdateLanguage()
    {
        Sprite sprite = null;
        switch(language)
        {
            case Language.English: sprite = englishFlag; break;
            case Language.Luxembourgish: sprite = luxFlag; break;
        }

        Image image = (Image) button.targetGraphic;
        image.sprite = sprite;
    }

    private void OnClick()
    {
        LocalizationManager.Instance.ChangeLanguage(language);
    }

    private void OnLanguageChanged(Language newLanguage)
    {
        Image image = (Image)button.targetGraphic;
        bool isHighlighted = newLanguage == language;
        image.materialForRendering.SetFloat("_OutlineEnabled", isHighlighted ? 1.0f : 0.0f);
    }
}
