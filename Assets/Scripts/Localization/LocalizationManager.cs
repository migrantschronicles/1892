using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
//using static UnityEditor.PlayerSettings.Switch;

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
            if (instance == null)
            {
                instance = new LocalizationManager();
            }

            return instance;
        }
    }

    public Language CurrentLanguage
    {
        get
        {
            if(LocalizationSettings.SelectedLocale.Identifier.Code.Contains("lb"))
            {
                return Language.Luxembourgish;
            }

            return Language.English;
        }
    }

    public delegate void OnLanguageChangedEvent(Language language);
    public event OnLanguageChangedEvent OnLanguageChanged;

    public LocalizationManager()
    {
        string languageCode = GetLanguageCode(CurrentLanguage);
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            // Set articy language
            ArticyDatabase.Localization.Language = languageCode;
        }
    }

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

    public string GetLanguageCode(Language language)
    {
        string languageCode = "";
        switch (language)
        {
            case Language.English: languageCode = "en"; break;
            case Language.Luxembourgish: languageCode = "lb"; break;
        }

        return languageCode;
    }

    public bool ChangeLanguage(Language language)
    {
        if(language == CurrentLanguage)
        {
            return true;
        }

        string languageCode = GetLanguageCode(language);
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
                    OnLanguageChanged?.Invoke(language);
                    return true;
                }
            }
        }

        return false;
    }
}
