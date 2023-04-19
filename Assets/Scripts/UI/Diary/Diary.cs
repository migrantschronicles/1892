using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OpenStatus
{
    Closed,
    Opening,
    Opened,
    Closing
}

public delegate void OnDiaryStatusChangedEvent(OpenStatus status);

public class Diary : MonoBehaviour
{
    public event OnDiaryStatusChangedEvent onDiaryStatusChanged;

    [SerializeField]
    private DiaryMarkerPanel markerPanel;
    [SerializeField]
    private Animator pagesController;

    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip pageClip;

    private DiaryContentPage currentPage;
    private DiaryContentPage nextPage;

    private OpenStatus diaryStatus;
    public OpenStatus Status
    {
        get { return diaryStatus; }
        set
        {
            diaryStatus = value;
            onDiaryStatusChanged?.Invoke(diaryStatus);
        }
    }

    public DiaryContentPage CurrentPage { get { return currentPage; } }

    public bool IsAnimationInProgress
    {
        get
        {
            return !(Status == OpenStatus.Opened || Status == OpenStatus.Closed) || (nextPage != null) ||
                (CurrentPage != null && currentPage.IsAnimationInProgress);
        }
    }

    public void OpenImmediately(DiaryContentPages pages)
    {
        Debug.Assert(Status == OpenStatus.Closed);
        currentPage = pages.CurrentPage;
        if(currentPage == null)
        {
            currentPage = pages.LastPage;
        }

        if(currentPage)
        {
            currentPage.OpenImmediately();
            if (currentPage.ContentPages)
            {
                currentPage.ContentPages.Active = true;
            }
        }

        Status = OpenStatus.Opened;
    }

    public void CloseImmediately()
    {
        Debug.Assert(Status == OpenStatus.Opened);
        // Not allowed to happen during page scrolling animation.
        Debug.Assert(nextPage == null);
        if (currentPage)
        {
            if(currentPage.ContentPages)
            {
                currentPage.ContentPages.Active = false;
            }

            currentPage.CloseImmediately();
            currentPage = null;
        }

        Status = OpenStatus.Closed;
    }

    public void Anim_StartPageAnimation()
    {
        // Can only happen during diary open or closing
        switch(Status)
        {
            case OpenStatus.Opening:
                nextPage.onStatusChanged += OnNextPageStatusChanged;
                nextPage.DiaryOpenToLeft();
                break;
        }
    }

    public void Anim_EndPageAnimation()
    {
        // Can only happen during diary open or closing
        switch (Status)
        {
            case OpenStatus.Closing:
                currentPage.onStatusChanged += OnCurrentPageStatusChanged;
                currentPage.DiaryCloseToRight();
                break;
        }
    }

    public void SetOpened(bool opened)
    {
        if(opened)
        {
            switch (Status)
            {
                case OpenStatus.Closed:
                case OpenStatus.Closing:
                    Status = OpenStatus.Opening;
                    break;
            }
        }
        else
        {
            switch(Status)
            {
                case OpenStatus.Opened:
                case OpenStatus.Opening:
                    Status = OpenStatus.Closing;
                    if(currentPage)
                    {
                        if(currentPage.ContentPages)
                        {
                            currentPage.ContentPages.Active = false;
                        }
                    }
                    break;
            }
        }
    }

    public void SetOpened(DiaryContentPage page)
    {
        if(Status != OpenStatus.Closed)
        {
            return;
        }

        SetOpened(true);
        nextPage = page;
    }

    public void SetOpened(DiaryContentPages pages)
    {
        DiaryContentPage pageToOpen = pages.CurrentPage;
        if (pageToOpen == null)
        {
            pageToOpen = pages.LastPage;
        }

        if(pageToOpen)
        {
            SetOpened(pageToOpen);
        }
    }

    public bool OpenPage(DiaryContentPage page)
    {
        if(Status != OpenStatus.Opened || page == currentPage || IsAnimationInProgress)
        {
            // Don't allow page switch during animation
            return false;
        }

        nextPage = page;
        nextPage.onStatusChanged += OnNextPageStatusChanged;
        currentPage.onStatusChanged += OnCurrentPageStatusChanged;

        DiaryContentPages currentPages = currentPage.ContentPages;
        DiaryContentPages nextPages = nextPage.ContentPages;
        bool isPageToRight = true;
        if(currentPages != nextPages)
        {
            currentPages.Active = false;

            int nextSiblingIndex = nextPages.transform.GetSiblingIndex();
            int currentSiblingIndex = currentPages.transform.GetSiblingIndex();
            if (nextSiblingIndex < currentSiblingIndex ||
                (nextSiblingIndex == currentSiblingIndex && nextPage.transform.GetSiblingIndex() < currentPage.transform.GetSiblingIndex()))
            {
                isPageToRight = false;
            }
        }
        else
        {
            int nextSiblingIndex = page.transform.GetSiblingIndex();
            int currentSiblingIndex = currentPage.transform.GetSiblingIndex();
            if (nextSiblingIndex < currentSiblingIndex ||
                (nextSiblingIndex == currentSiblingIndex && nextPage.transform.GetSiblingIndex() < currentPage.transform.GetSiblingIndex()))
            {
                isPageToRight = false;
            }
        }

        if(isPageToRight)
        {
            currentPage.CloseToLeft();
            nextPage.OpenToLeft();
        }
        else
        {
            currentPage.CloseToRight();
            nextPage.OpenToRight();
        }

        pagesController.SetTrigger(isPageToRight ? "FlipLeft" : "FlipRight");
        AudioManager.Instance.PlayFX(pageClip);

        return true;
    }

    public void OnOpeningAnimationFinished()
    {
        Debug.Assert(Status == OpenStatus.Opening);
        Status = OpenStatus.Opened;
    }

    public void OnClosingAnimationFinished()
    {
        Debug.Assert(Status == OpenStatus.Closing);
        Status = OpenStatus.Closed;
    }

    private void OnNextPageStatusChanged(OpenStatus status)
    {
        if (status == OpenStatus.Opened)
        {
            nextPage.onStatusChanged -= OnNextPageStatusChanged;
            if(currentPage != null)
            {
                // Remove callback, in case next page is opened and current page is closed at the same time.
                currentPage.onStatusChanged -= OnCurrentPageStatusChanged;
            }
            currentPage = nextPage;
            nextPage = null;
            if (currentPage.ContentPages)
            {
                currentPage.ContentPages.Active = true;
                currentPage.ContentPages.CurrentPage = currentPage;
            }
        }
    }

    private void OnCurrentPageStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Closed)
        {
            currentPage.onStatusChanged -= OnCurrentPageStatusChanged;
            currentPage = null;
        }
    }

    public bool OpenPrevPageOfContentPages()
    {
        if(currentPage == null || currentPage.IsFirstPageOfContentPages)
        {
            return false;
        }

        DiaryContentPage prevPage = currentPage.transform.parent.GetChild(currentPage.transform.GetSiblingIndex() - 1).GetComponent<DiaryContentPage>();
        if(prevPage != null)
        {
            return OpenPage(prevPage);
        }

        return false;
    }

    public bool OpenNextPageOfContentPages()
    {
        if (currentPage == null || currentPage.IsLastPageOfContentPages)
        {
            return false;
        }

        DiaryContentPage pageAfter = currentPage.transform.parent.GetChild(currentPage.transform.GetSiblingIndex() + 1).GetComponent<DiaryContentPage>();
        if (pageAfter != null)
        {
            return OpenPage(pageAfter);
        }

        return false;
    }

    public void OpenContentPages(DiaryContentPages pages)
    {
        if(pages.CurrentPage)
        {
            OpenPage(pages.CurrentPage);
        }
        else
        {
            DiaryContentPage lastPage = pages.LastPage;
            if(lastPage)
            {
                OpenPage(lastPage);
            }
        }
    }

    public void SetMarkersClosed(bool closed)
    {
        markerPanel.SetClosed(closed);
    }
}
