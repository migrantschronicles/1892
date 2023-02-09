using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Diary2 : MonoBehaviour
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
    [SerializeField]
    private Button centerButton;
    [SerializeField]
    private GameObject currentLocation;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip pageClip;

    private Dictionary<string, LocationMarker> locationMarkers;
    private DiaryPageLink openPage;

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
        currentLocationText.text = LevelInstance.Instance.LocationName;
        openPage = inventoryPage.activeSelf ? DiaryPageLink.Inventory :
            (healthPage.activeSelf ? DiaryPageLink.Health :
            (diaryPage.activeSelf ? DiaryPageLink.Diary :
            (mapPage.activeSelf ? DiaryPageLink.Map :
            DiaryPageLink.Settings)));
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
        openPage = DiaryPageLink.Inventory;
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenDiaryPage()
    {
        CloseAll();
        prevPageButton.gameObject.SetActive(true);
        nextPageButton.gameObject.SetActive(true);
        diaryPage.SetActive(true);
        //diaryPages.OnVisiblityChanged(true);
        openPage = DiaryPageLink.Diary;
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenHealthPage()
    {
        CloseAll();
        healthPage.SetActive(true);
        openPage = DiaryPageLink.Health;
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenSettingsPage()
    {
        CloseAll();
        settingsPage.SetActive(true);
        openPage = DiaryPageLink.Settings;
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenMapPage()
    {
        CloseAll();
        mapPage.SetActive(true);
        openPage = DiaryPageLink.Map;
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenPage(DiaryPageLink page)
    {
        switch(page)
        {
            case DiaryPageLink.Inventory: OpenInventoryPage(); break;
            case DiaryPageLink.Health: OpenHealthPage(); break;
            case DiaryPageLink.Map: OpenMapPage(); break;
            case DiaryPageLink.Diary: OpenDiaryPage(); break;
            case DiaryPageLink.Settings: OpenSettingsPage(); break;
        }
    }

    private void CloseAll()
    {
        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        mapPage.SetActive(false);
        settingsPage.SetActive(false);
        diaryPage.SetActive(false);
        //diaryPages.OnVisiblityChanged(false);
        prevPageButton.gameObject.SetActive(false);
        nextPageButton.gameObject.SetActive(false);
    }

    public void SetVisible(bool visible, DiaryPageLink page = DiaryPageLink.Inventory)
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
        centerButton.gameObject.SetActive(false);
        currentLocation.SetActive(false);
    }

    public void PrepareForDiaryScreenshot(DiaryEntryData entry)
    {
        inventoryPage.SetActive(false);
        healthPage.SetActive(false);
        diaryPage.SetActive(true);
        settingsPage.SetActive(false);
        mapPage.SetActive(false);
        diaryPages.PrepareForDiaryScreenshot(entry);
    }

    public void ResetFromScreenshot()
    {
        mapZoom.ResetFromScreenshot();
        diaryPages.ResetFromScreenshot();
        inventoryPage.SetActive(openPage == DiaryPageLink.Inventory);
        healthPage.SetActive(openPage == DiaryPageLink.Health);
        diaryPage.SetActive(openPage == DiaryPageLink.Diary);
        mapPage.SetActive(openPage == DiaryPageLink.Map);
        settingsPage.SetActive(openPage == DiaryPageLink.Settings);
        centerButton.gameObject.SetActive(true);
        currentLocation.SetActive(true);
    }

    public void GeneratePDF()
    {
        NewGameManager.Instance.GeneratePDF();
    }
}
