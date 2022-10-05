using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum DiaryPageType
{
    Inventory,
    Health,
    Diary,
    Map,
    Settings
}

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
    private MapZoom mapZoom;
    [SerializeField]
    private GameObject locationMarkerParent;
    [SerializeField]
    private Text currentLocationText;
    [SerializeField]
    private DiaryPages diaryPages;

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

    private void Start()
    {
        currentLocationText.text = NewGameManager.Instance.currentLocation;
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

    public void OpenPage(DiaryPageType page)
    {
        switch(page)
        {
            case DiaryPageType.Inventory: OpenInventoryPage(); break;
            case DiaryPageType.Health: OpenHealthPage(); break;
            case DiaryPageType.Map: OpenMapPage(); break;
            case DiaryPageType.Diary: OpenDiaryPage(); break;
            case DiaryPageType.Settings: OpenSettingsPage(); break;
        }
    }

    private void CloseAll()
    {
        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        mapPage.SetActive(false);
        settingsPage.SetActive(false);
        diaryPage.SetActive(false);
        diaryPages.StopAnimators();
    }

    public void SetVisible(bool visible, DiaryPageType page = DiaryPageType.Inventory)
    {
        gameObject.SetActive(visible);
        if(visible)
        {
            mapZoom.ResetInitialZoom();
            OpenPage(page);
        }
        else
        {
            diaryPages.StopAnimators();
        }
    }
}
