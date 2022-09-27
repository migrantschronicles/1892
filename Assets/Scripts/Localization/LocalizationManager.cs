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

    public string GetLocalizedString(LocalizedString localizedString)
    {
        return localizedString.GetLocalizedString();
    }

    public string GetLocalizedString(LocalizedString localizedString, params object[] args)
    {
        return localizedString.GetLocalizedString(args);
    }

    public string GetLocalizedString(LocalizedString localizedString, IList<object> args)
    {
        return localizedString.GetLocalizedString(args);
    }

    public bool ChangeLanguage(Language language)
    {
        string languageCode = "";
        switch(language)
        {
            case Language.English: languageCode = "en"; break;
            case Language.Luxembourgish: languageCode = "lb"; break;
        }

        if(!string.IsNullOrWhiteSpace(languageCode))
        {
            LocaleIdentifier identifier = new LocaleIdentifier(languageCode);
            for(int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
                if(locale.Identifier == identifier)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    return true;
                }
            }
        }

        return false;
    }
}
