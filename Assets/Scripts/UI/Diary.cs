using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Diary : MonoBehaviour
{
    [SerializeField]
    private GameObject inventoryPage;
    [SerializeField]
    private GameObject diaryPage;
    [SerializeField]
    private GameObject healthPage;
    [SerializeField]
    private GameObject mapPage;
    [SerializeField]
    private GameObject settingsPage;
    [SerializeField]
    private GameObject locationMarkerParent;

    private Dictionary<string, LocationMarker> locationMarkers;

    public Dictionary<string, LocationMarker> LocationMarkers
    {
        get
        {
            if(locationMarkers == null)
            {
                GatherLocationMarkers();
            }

            return locationMarkers;
        }
    }

    public IEnumerable<string> LocationStrings
    {
        get
        {
            return LocationMarkers.Keys;
        }
    }

    public IEnumerable<LocationMarker> LocationMarkerObjects
    {
        get
        {
            return LocationMarkers.Values;
        }
    }

    public IEnumerable<GameObject> LocationMarkersGO
    {
        get
        {
            return LocationMarkers.Values.Select(marker => marker.gameObject);
        }
    }

    private void Awake()
    {
        if (locationMarkers == null)
        {
            GatherLocationMarkers();
        }
    }

    private void GatherLocationMarkers()
    {
        locationMarkers = new Dictionary<string, LocationMarker>();
        for(int i = 0; i < locationMarkerParent.transform.childCount; ++i)
        {
            LocationMarker marker = locationMarkerParent.transform.GetChild(i).GetComponent<LocationMarker>();
            if(marker != null)
            {
                locationMarkers.Add(marker.LocationName, marker);
            }
        }
    }

    public void OpenInventoryPage()
    {
        CloseAll();
        inventoryPage.SetActive(true);
    }

    public void OpenDiaryPage()
    {
        CloseAll();
        diaryPage.SetActive(true);
    }

    public void OpenHealthPage()
    {
        CloseAll();
        healthPage.SetActive(true);
    }

    public void OpenSettingsPage()
    {
        CloseAll();
        settingsPage.SetActive(true);
    }

    public void OpenMapPage()
    {
        CloseAll();
        mapPage.SetActive(true);
    }

    private void CloseAll()
    {
        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        mapPage.SetActive(false);
        settingsPage.SetActive(false);
        diaryPage.SetActive(false);
    }
}
