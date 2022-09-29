using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public void SetVisible(bool visible, DiaryPageType page = DiaryPageType.Inventory)
    {
        gameObject.SetActive(visible);
        if(visible)
        {
            mapZoom.ResetInitialZoom();
            OpenPage(page);
        }
    }
}
