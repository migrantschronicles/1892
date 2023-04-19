using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public enum Language
{
    English,
    Luxembourgish
}

public class LocalizationManager
{
    private static LocalizationManager instance;

    public static LocalizationManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new LocalizationManager();
            }

            return instance;
        }
    }

    public Language Language { get; private set; } = Language.English;

    public delegate void OnLanguageChangedEvent(Language language);
    public event OnLanguageChangedEvent OnLanguageChanged;

    public string GetLocalizedString(LocalizedString localizedString)
    {
        if(localizedString.IsEmpty)
        {
            return "";
        }

        return localizedString.GetLocalizedString();
    }

    public string GetLocalizedString(LocalizedString localizedString, params object[] args)
    {
        if (localizedString.IsEmpty)
        {
            return "";
        }

        return localizedString.GetLocalizedString(args);
    }

    public string GetLocalizedString(LocalizedString localizedString, IList<object> args)
    {
        if (localizedString.IsEmpty)
        {
            return "";
        }

        return localizedString.GetLocalizedString(args);
    }

    public bool ChangeLanguage(Language language)
    {
        if(language == Language)
        {
            return true;
        }

        Language = language;
        string languageCode = "";
        switch(language)
        {
            case Language.English: languageCode = "en"; break;
            case Language.Luxembourgish: languageCode = "lb"; break;
        }

        if(!string.IsNullOrWhiteSpace(languageCode))
        {
            // Set articy language
            ArticyDatabase.Localization.Language = languageCode;

            // Set application language
            LocaleIdentifier identifier = new LocaleIdentifier(languageCode);
            for(int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
                if(locale.Identifier == identifier)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    OnLanguageChanged?.Invoke(Language);
                    return true;
                }
            }
        }

        return false;
    }
}
