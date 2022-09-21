using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class TestLocalization : MonoBehaviour
{
    public LocalizedString localizedString;

    private Button button;
    private Text text;

    private void Start()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();

        button.onClick.AddListener(OnClick);
        text.text = localizedString.GetLocalizedString();
        localizedString.StringChanged += (value) => text.text = value;
    }

    private void OnClick()
    {
        LocaleIdentifier selectedIdentifier = LocalizationSettings.SelectedLocale.Identifier;
        for(int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
            LocaleIdentifier identifier = locale.Identifier;
            if(identifier == selectedIdentifier)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(i + 1) % LocalizationSettings.AvailableLocales.Locales.Count];
                break;
            }
        }
    }
}
