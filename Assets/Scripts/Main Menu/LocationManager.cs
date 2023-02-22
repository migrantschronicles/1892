using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LocationInfo
{
    public string displayName;
    public string technicalName;
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

        return null;
    }
}
