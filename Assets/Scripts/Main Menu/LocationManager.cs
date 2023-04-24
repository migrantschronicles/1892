using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum Continent
{
    None,
    Europe,
    America,
    Ship
}

[System.Serializable]
public class LocationInfo
{
    public string displayName;
    public string technicalName;
    public Continent continent;
    public LocalizedString localizedString;
}

public class LocationManager : MonoBehaviour
{
    [SerializeField]
    private LocationInfo[] infos;

    public string GetLocationByTechnicalName(string technicalName)
    {
        foreach(LocationInfo info in infos)
        {
            if(info.technicalName == technicalName)
            {
                return info.displayName;
            }
        }

        Debug.LogError($"Did not find a location for {technicalName}");
        return null;
    }

    public string GetLocalizedName(string location)
    {
        foreach(LocationInfo info in infos)
        {
            if(info.displayName == location)
            {
                return LocalizationManager.Instance.GetLocalizedString(info.localizedString);
            }
        }

        Debug.LogError($"LocationManager::GetLocalizedName: {location} not found");
        return null;
    }

    public Continent GetContinent(string location)
    {
        foreach(LocationInfo info in infos)
        {
            if(info.displayName == location)
            {
                return info.continent;
            }
        }

        Debug.LogError($"LocationManager::GetContinent: {location} not found");
        return Continent.None;
    }

    public bool IsFromEuropeToAmerica(string from, string to)
    {
        Continent current = GetContinent(from);
        Continent next = GetContinent(to);
        return current == Continent.Europe && next == Continent.America;
    }

    public Currency GetCurrencyForLocation(string location)
    {
        foreach(LocationInfo info in infos)
        {
            if(info.displayName == location)
            {
                return info.continent == Continent.America ? Currency.Dollar : Currency.Franc;
            }
        }

        return Currency.Franc;
    }
}
