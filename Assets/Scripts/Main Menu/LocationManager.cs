using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocationInfo
{
    public string displayName;
    public string technicalName;
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
}
