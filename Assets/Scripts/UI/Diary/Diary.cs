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

    public IEnumerable<LocationMarker> LocationMarkerObjects
    {
        get
        {
            ///@todo
            return new List<LocationMarker>();
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
                        currentPage.onStatusChanged += OnCurrentPageStatusChanged;
                        currentPage.CloseToRight();
                    }
                    break;
            }
        }
    }

    public void SetOpened(DiaryContentPage page)
    {
        switch(Status)
        {
            case OpenStatus.Closed:
                SetOpened(true);
                nextPage = page;
                nextPage.onStatusChanged += OnNextPageStatusChanged;
                nextPage.OpenToLeft();
                break;

            case OpenStatus.Opened:
                nextPage = page;
                ///@todo
                break;
        }
    }

    public void OpenPage(DiaryContentPage page)
    {
        if(Status != OpenStatus.Opened || page == currentPage)
        {
            return;
        }

        nextPage = page;
        nextPage.onStatusChanged += OnNextPageStatusChanged;
        currentPage.onStatusChanged += OnCurrentPageStatusChanged;

        DiaryContentPages currentPages = currentPage.ContentPages;
        DiaryContentPages nextPages = nextPage.ContentPages;
        bool isPageToRight = true;
        if(currentPages != nextPages)
        {
            if(currentPages.DiaryMarker)
            {
                currentPages.DiaryMarker.SetActive(false);
            }

            int nextSiblingIndex = nextPages.transform.GetSiblingIndex();
            int currentSiblingIndex = currentPages.transform.GetSiblingIndex();
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
            if (currentPage.ContentPages && currentPage.ContentPages.DiaryMarker)
            {
                currentPage.ContentPages.DiaryMarker.SetActive(true);
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
}
