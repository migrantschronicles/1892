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
    [SerializeField]
    private GameObject tempMapButtonsDev;
    [SerializeField]
    private GameObject returnToMainMenuPrefab;

    public Diary Diary { get { return diary; } }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    private void Start()
    {
        if(immediatelyOpenedPages)
        {
            OpenImmediately(immediatelyOpenedPages);
        }

//#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
        tempMapButtonsDev.SetActive(false);
//#endif
    }

    private void OnEnable()
    {
        switch (diary.Status)
        {
            case OpenStatus.Opened:
                diaryAnimator.SetTrigger("OpenImmediately");
                diaryAnimator.SetBool("ImmediatelyFix", true);
                break;
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

    public void Anim_EndPageAnimation()
    {
        diary.Anim_EndPageAnimation();
    }

    public void OnReturnToMainMenu()
    {
        gameObject.SetActive(false);
        LevelInstance.Instance.SetBackButtonVisible(false);

        GameObject popupGO = Instantiate(returnToMainMenuPrefab, LevelInstance.Instance.Canvas.transform);
        ReturnToMainMenuPopup popup = popupGO.GetComponent<ReturnToMainMenuPopup>();
        popup.OnBack += (popup) =>
        {
            Destroy(popupGO);
            gameObject.SetActive(true);
            ///@todo LevelInstance should have UpdateBackButtonVisibililty(), and check whether button should be visible based on mode and diary status
            LevelInstance.Instance.SetBackButtonVisible(true);
        };
        popup.OnMainMenu += (popup) =>
        {
            Destroy(popupGO);
            if(NewGameManager.Instance.wantsEndGame)
            {
                NewGameManager.Instance.EndGameAndReturnToMainMenu();
            }
            else
            {
                NewGameManager.Instance.ReturnToMainMenu();
            }
        };
    }
}
