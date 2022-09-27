using System.Collections;
using System.Collections.Generic;
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
