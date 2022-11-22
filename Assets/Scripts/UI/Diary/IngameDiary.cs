using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DiaryPageLink
{
    Inventory,
    Health,
    Diary,
    Map,
    Settings
}

public class IngameDiary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Diary diary;
    [SerializeField]
    private DiaryContentPage defaultPage;
    [SerializeField]
    private GameObject locationMarkerParent;
    [SerializeField]
    private DiaryContentPages inventoryPages;
    [SerializeField]
    private DiaryContentPages healthPages;
    [SerializeField]
    private DiaryContentPages diaryPages;
    [SerializeField]
    private DiaryContentPages mapPages;
    [SerializeField]
    private DiaryContentPages settingsPages;
    [SerializeField]
    private DiaryContentPages immediatelyOpenedPages;

    private Dictionary<string, LocationMarker> locationMarkers;
    private DiaryContentPage screenshotPrevPage;

    public Diary Diary { get { return diary; } }

    public Dictionary<string, LocationMarker> LocationMarkers
    {
        get
        {
            if (locationMarkers == null)
            {
                GatherLocationMarkers();
            }

            return locationMarkers;
        }
    }

    public IEnumerable<string> LocationStrings { get { return LocationMarkers.Keys; } }
    public IEnumerable<LocationMarker> LocationMarkerObjects { get { return LocationMarkers.Values; } }
    public IEnumerable<GameObject> LocationMarkersGO
    {
        get
        {
            return LocationMarkers.Values.Select(marker => marker.gameObject);
        }
    }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
        if (locationMarkers == null)
        {
            GatherLocationMarkers();
        }
    }

    private void Start()
    {
        if(immediatelyOpenedPages)
        {
            OpenImmediately(immediatelyOpenedPages);
        }
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch(status)
        {
            case OpenStatus.Opening:
                diaryAnimator.SetBool("Opened", true);
                diaryAnimator.SetBool("ImmediatelyFix", true);
                StartCoroutine(WaitForAnimationEvents());
                break;

            case OpenStatus.Closing:
                diaryAnimator.SetBool("Opened", false);
                diaryAnimator.SetBool("ImmediatelyFix", false);
                StartCoroutine(WaitForAnimationEvents());
                break;
        }
    }

    private IEnumerator WaitForAnimationEvents()
    {
        while((diary.Status == OpenStatus.Opening && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryOpened"))
            || (diary.Status == OpenStatus.Closing && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryClosed")))
        {
            yield return null;
        }

        switch(diary.Status)
        {
            case OpenStatus.Opening:
                diary.OnOpeningAnimationFinished();
                break;

            case OpenStatus.Closing:
                diary.OnClosingAnimationFinished();
                break;
        }
    }

    public void SetOpened(bool opened)
    {
        if(opened)
        {
            diary.SetOpened(defaultPage);
        }
        else
        {
            diary.SetOpened(opened);
        }
    }

    private DiaryContentPages GetContentPagesFromPageLink(DiaryPageLink page)
    {
        DiaryContentPages pages = null;
        switch (page)
        {
            case DiaryPageLink.Inventory: pages = inventoryPages; break;
            case DiaryPageLink.Health: pages = healthPages; break;
            case DiaryPageLink.Diary: pages = diaryPages; break;
            case DiaryPageLink.Map: pages = mapPages; break;
            case DiaryPageLink.Settings: pages = settingsPages; break;
        }

        return pages;
    }

    public void SetOpened(DiaryPageLink page)
    {
        DiaryContentPages pages = GetContentPagesFromPageLink(page);
        diary.SetOpened(pages);
    }

    private IEnumerator SwitchKeepOpenAndOpened(bool opened)
    {
        // Wait 1 frame.
        yield return null;

        diaryAnimator.SetBool("ImmediatelyFix", !opened);
        diaryAnimator.SetBool("Opened", opened);
    }

    public void OpenImmediately(DiaryPageLink page)
    {
        Debug.Assert(Diary.Status == OpenStatus.Closed);

        DiaryContentPages pages = GetContentPagesFromPageLink(page);
        OpenImmediately(pages);
    }

    public void OpenImmediately(DiaryContentPages pages)
    {
        Debug.Assert(Diary.Status == OpenStatus.Closed);

        diaryAnimator.SetTrigger("OpenImmediately");
        diaryAnimator.SetBool("ImmediatelyFix", true);
        diary.OpenImmediately(pages);
        StartCoroutine(SwitchKeepOpenAndOpened(true));
    }

    public void CloseImmediately()
    {
        Debug.Assert(Diary.Status == OpenStatus.Opened);
        diaryAnimator.SetTrigger("CloseImmediately");
        diaryAnimator.SetBool("ImmediatelyFix", false);
        StartCoroutine(SwitchKeepOpenAndOpened(false));
        diary.CloseImmediately();
    }

    private void GatherLocationMarkers()
    {
        locationMarkers = new Dictionary<string, LocationMarker>();
        for (int i = 0; i < locationMarkerParent.transform.childCount; ++i)
        {
            LocationMarker marker = locationMarkerParent.transform.GetChild(i).GetComponent<LocationMarker>();
            if (marker != null)
            {
                locationMarkers.Add(marker.LocationName, marker);
            }
        }
    }

    public void PrepareForMapScreenshot()
    {
        screenshotPrevPage = diary.CurrentPage;
        if(diary.CurrentPage)
        {
            diary.CurrentPage.gameObject.SetActive(false);
            if(diary.CurrentPage.ContentPages)
            {
                diary.CurrentPage.ContentPages.ActiveSilent = false;
            }
        }

        mapPages.LastPage.gameObject.SetActive(true);
        mapPages.ActiveSilent = true;
        mapPages.GetComponentInChildren<MapZoom>().PrepareForMapScreenshot();
        mapPages.LastPage.GetComponent<MapPage>().PrepareForMapScreenshot();
    }

    public void PrepareForDiaryScreenshot(DiaryEntryData data)
    {
        screenshotPrevPage = diary.CurrentPage;
        if (diary.CurrentPage)
        {
            diary.CurrentPage.gameObject.SetActive(false);
            if (diary.CurrentPage.ContentPages)
            {
                diary.CurrentPage.ContentPages.ActiveSilent = false;
            }
        }

        diaryPages.ActiveSilent = true;
        diaryPages.GetComponentInChildren<DiaryPages>().PrepareForDiaryScreenshot(data);
    }

    public void ResetFromScreenshot()
    {
        if(mapPages.Active)
        {
            mapPages.GetComponentInChildren<MapZoom>().ResetFromScreenshot();
            mapPages.LastPage.GetComponent<MapPage>().ResetFromScreenshot();
            mapPages.LastPage.gameObject.SetActive(false);
            mapPages.ActiveSilent = false;
        }

        if(diaryPages.Active)
        {
            diaryPages.GetComponentInChildren<DiaryPages>().ResetFromScreenshot();
            diaryPages.ActiveSilent = false;
        }

        if(screenshotPrevPage)
        {
            Debug.Log(screenshotPrevPage);
            screenshotPrevPage.gameObject.SetActive(true);
            screenshotPrevPage.GetComponent<Animator>().SetTrigger("OpenImmediately");
            if(screenshotPrevPage.ContentPages)
            {
                screenshotPrevPage.ContentPages.ActiveSilent = true;
            }

            screenshotPrevPage = null;
        }
        else
        {
            Debug.Log("NULL");
        }
    }

    public void GeneratePDF()
    {
        NewGameManager.Instance.GeneratePDF();
    }

    public void SetMarkersClosed()
    {
        diary.SetMarkersClosed(true);
    }

    public void SetMarkersOpened()
    {
        diary.SetMarkersClosed(false);
    }

    public void Anim_StartPageAnimation()
    {
        diary.Anim_StartPageAnimation();
    }
}
