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
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private Button nextPageButton;

    private Dictionary<string, LocationMarker> locationMarkers;
    private DiaryPageType openPage;

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
        openPage = inventoryPage.activeSelf ? DiaryPageType.Inventory :
            (healthPage.activeSelf ? DiaryPageType.Health :
            (diaryPage.activeSelf ? DiaryPageType.Diary :
            (mapPage.activeSelf ? DiaryPageType.Map :
            DiaryPageType.Settings)));
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
        openPage = DiaryPageType.Inventory;
    }

    public void OpenDiaryPage()
    {
        CloseAll();
        prevPageButton.gameObject.SetActive(true);
        nextPageButton.gameObject.SetActive(true);
        diaryPage.SetActive(true);
        diaryPages.OnVisiblityChanged(true);
        openPage = DiaryPageType.Diary;
    }

    public void OpenHealthPage()
    {
        CloseAll();
        healthPage.SetActive(true);
        openPage = DiaryPageType.Health;
    }

    public void OpenSettingsPage()
    {
        CloseAll();
        settingsPage.SetActive(true);
        openPage = DiaryPageType.Settings;
    }

    public void OpenMapPage()
    {
        CloseAll();
        mapPage.SetActive(true);
        openPage = DiaryPageType.Map;
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
        diaryPages.OnVisiblityChanged(false);
        if (diaryPage.activeSelf)
        {
            LevelInstance.Instance.ConditionallyTakeDiaryEntryScreenshot();
        }

        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        mapPage.SetActive(false);
        settingsPage.SetActive(false);
        diaryPage.SetActive(false);
        prevPageButton.gameObject.SetActive(false);
        nextPageButton.gameObject.SetActive(false);
    }

    public void SetVisible(bool visible, DiaryPageType page = DiaryPageType.Inventory)
    {
        if(visible)
        {
            mapZoom.ResetInitialZoom();
            OpenPage(page);
        }
        else
        {
            diaryPages.StopAnimators(true);
        }
        gameObject.SetActive(visible);
    }

    public void PrepareForMapScreenshot()
    {
        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        diaryPage.SetActive(false);
        settingsPage.SetActive(false);
        mapPage.SetActive(true);
        mapZoom.PrepareForMapScreenshot();
    }

    public void ResetFromMapScreenshot()
    {
        mapZoom.ResetFromMapScreenshot();
        inventoryPage.SetActive(openPage == DiaryPageType.Inventory);
        healthPage.SetActive(openPage == DiaryPageType.Health);
        diaryPage.SetActive(openPage == DiaryPageType.Diary);
        mapPage.SetActive(openPage == DiaryPageType.Map);
        settingsPage.SetActive(openPage == DiaryPageType.Settings);
    }

    public void GeneratePDF()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
