using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/**
 * Takes care of adding the diary pages.
 * The game manager has a list of diary entries that the user has written.
 * Each entry can span over multiple pages.
 * This DiaryPages has allowNewEntriesOnSameDoublePage, which states whether new entries have to begin on a new double page (so always left),
 * even if that means that there may be one page empty (if the previous entry ends on the left page). This is a global switch to enforce this behavior.
 * Each diary entry can decide for its own if it wants to be on a new page or if it can be on the same double page, but the DiaryPages can disallow it.
 * (Maybe there are main diary entries which should be on a new double page, and smaller follow up entries which do not need to be on its own double page).
 * 
 * DIARY ENTRY
 * To create a new diary entry, go to Assets/DiaryEntries and right click: Create > Scriptable Objects > DiaryEntry.
 * In an entry, you can set whether this entry wants to be on a new double page.
 * Then you can add pages (each entry can consist of multiple pages).
 * For each page, you need to select which page prefab you want to use (which layout of the page).
 * The prefabs are in Prefabs/DiaryEntries (but don't use DiaryPage).
 * You have multiple layouts for an option:
 *  - FirstPage: Has a date on the top right, and text on the rest of the page, useful for the first page of each diary entry.
 *  - ImageTopPage: Has an image on the top of the page, and then text below it.
 *  - FullImagePage: Has an image on the whole page.
 *  Then you can set the text and / or the image, depending on the layout of the page.
 */
public class DiaryPages : MonoBehaviour
{
    [SerializeField]
    private GameObject contentParent;
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private Button nextPageButton;
    [SerializeField]
    private Button contentLeftButton;
    [SerializeField]
    private Button contentRightButton;
    [SerializeField]
    private AudioClip nextPageClip;
    [SerializeField]
    private AudioClip prevPageClip;
    [SerializeField]
    private GameObject contentPagePrefab;
    [SerializeField]
    private DiaryContentPages contentPages;

    private List<ElementAnimator> currentAnimators = new List<ElementAnimator>();
    private DiaryContentPage lastAddedPage;
    private DiaryContentPage screenshotPage;

    private void Awake()
    {
        prevPageButton.onClick.AddListener(OpenPrevDoublePage);
        nextPageButton.onClick.AddListener(OpenNextDoublePage);
        contentLeftButton.onClick.AddListener(() => StopAnimators(true));
        contentRightButton.onClick.AddListener(() => StopAnimators(true));
        contentPages.onActiveStatusChanged += OnContentPagesActiveStatusChanged;
    }

    private void Start()
    {
        foreach(DiaryEntry entry in NewGameManager.Instance.DiaryEntries)
        {
            AddEntry(entry);
        }
        NewGameManager.Instance.onDiaryEntryAdded += AddEntry;
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onDiaryEntryAdded -= AddEntry;
        }
    }

    public void OnContentPagesActiveStatusChanged(bool visible)
    {
        if (visible)
        {
            UpdateButtons();
        }
        else
        {
            StopAnimators(false);
        }
    }

    private void UpdateButtons()
    {
        if(NewGameManager.Instance.wantsEndGame)
        {
            nextPageButton.gameObject.SetActive(false);
            prevPageButton.gameObject.SetActive(false);
        }
        else
        {
            DiaryContentPage currentPage = LevelInstance.Instance.IngameDiary.Diary.CurrentPage;
            if (currentPage && currentPage.ContentPages == contentPages)
            {
                prevPageButton.gameObject.SetActive(!currentPage.IsFirstPageOfContentPages);
                nextPageButton.gameObject.SetActive(!currentPage.IsLastPageOfContentPages);
            }
        }
    }

    private void CreatePageContent(DiaryPageData data, Transform parent, bool animated)
    {
        if(!data.IsValid)
        {
            return;
        }

        GameObject newPageContent = Instantiate(data.prefab, parent);

        foreach (DiaryPageDrawing drawing in data.drawings)
        {
            if (drawing.IsEnabled)
            {
                GameObject drawingGO = AddDrawingToPage(newPageContent, drawing);
                if(animated)
                {
                    currentAnimators.Add(ImageElementAnimator.FromImage(this, drawingGO.GetComponentInChildren<Image>()));
                }
            }
        }

        IDiaryPage diaryPage = newPageContent.GetComponent<IDiaryPage>();
        diaryPage.SetData(data);

        if(animated)
        {
            currentAnimators.AddRange(diaryPage.CreateAnimators());
        }
    }

    public void AddEntry(DiaryEntry entry)
    {
        StopAnimators(false);
        if(lastAddedPage)
        {
            lastAddedPage.onStatusChanged -= OnDiaryContentPageStatusChanged;
            lastAddedPage = null;
        }

        string date = NewGameManager.Instance.date.ToString("d MMMM yyyy");
        entry.leftPage.Date = date;
        entry.rightPage.Date = date;

        GameObject newContentPageGO = Instantiate(contentPagePrefab, contentParent.transform);
        DiaryContentPage newContentPage = newContentPageGO.GetComponent<DiaryContentPage>();
        CreatePageContent(entry.leftPage, newContentPage.LeftPage.transform, true);
        CreatePageContent(entry.rightPage, newContentPage.RightPage.transform, true);

        lastAddedPage = newContentPage;
        lastAddedPage.onStatusChanged += OnDiaryContentPageStatusChanged;

        DiaryContentPage currentPage = LevelInstance.Instance.IngameDiary.Diary.CurrentPage;
        if (LevelInstance.Instance.IngameDiary.gameObject.activeSelf && currentPage && currentPage.ContentPages == contentPages)
        {
            // The diary is already opened.
            OnDiaryContentPageStatusChanged(OpenStatus.Opened);
        }
        else
        {
            // Set the current page of the diary content pages so that the new page is opened the next time that the diary entries are opened.
            contentPages.CurrentPage = newContentPage;
            newContentPage.gameObject.SetActive(false);
        }
    }

    private void OnDiaryContentPageStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Opening || status == OpenStatus.Opened)
        {
            lastAddedPage.onStatusChanged -= OnDiaryContentPageStatusChanged;
            lastAddedPage = null;

            if (currentAnimators.Count > 0)
            {
                StartAnimator(currentAnimators[0]);
            }
        }
    }

    private GameObject AddDrawingToPage(GameObject page, DiaryPageDrawing drawing)
    {
        // Create the sketch drawing.
        GameObject newSketchGO = new GameObject("SketchDrawing", typeof(RectTransform), typeof(Image));
        Image newSketchImage = newSketchGO.GetComponent<Image>();
        newSketchImage.color = Color.white;
        newSketchImage.raycastTarget = false;
        newSketchImage.sprite = drawing.image;
        newSketchImage.preserveAspect = drawing.preserveAspect;

        // Attach it to the page.
        newSketchGO.transform.SetParent(page.transform, false);
        newSketchGO.transform.SetSiblingIndex(0);
        RectTransform rectTransform = newSketchGO.GetComponent<RectTransform>();
        Vector2 anchor = Vector2.zero;
        Vector2 position = Vector2.zero;
        Vector2 size = drawing.overrideSize ? drawing.size : drawing.image.rect.size;

        switch(drawing.location)
        {
            case DiaryPageDrawingLocation.BottomLeft:
            {
                anchor = Vector2.zero;
                position = size / 2;
                break;
            }

            case DiaryPageDrawingLocation.BottomRight:
            {
                anchor = new Vector2(1, 0);
                position = new Vector2(-size.x / 2, size.y / 2);
                break;
            }

            case DiaryPageDrawingLocation.TopLeft:
            {
                anchor = new Vector2(0, 1);
                position = new Vector2(size.x / 2, -size.y / 2);
                break;
            }

            case DiaryPageDrawingLocation.TopRight:
            {
                anchor = new Vector2(1, 1);
                position = -size / 2;
                break;
            }

            case DiaryPageDrawingLocation.Center:
            {
                anchor = new Vector2(0.5f, 0.5f);
                position = Vector2.zero;
                break;
            }
        }

        // Add the specified offset
        position += drawing.offset;

        // Set the position and size.
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        return newSketchGO;
    }

    public void OpenPrevDoublePage()
    {
        StopAnimators(true);
        
        if(LevelInstance.Instance.IngameDiary.Diary.OpenPrevPageOfContentPages())
        {
            AudioManager.Instance.PlayFX(prevPageClip);
        }
    }

    public void OpenNextDoublePage()
    {
        StopAnimators(true);

        if(LevelInstance.Instance.IngameDiary.Diary.OpenNextPageOfContentPages())
        {
            AudioManager.Instance.PlayFX(nextPageClip);
        }
    }

    public void StopAnimators(bool takeScreenshot)
    {
        foreach(ElementAnimator animator in currentAnimators)
        {
            animator.Finish();
        }
        currentAnimators.Clear();

        if(takeScreenshot)
        {
            AudioManager.Instance.PlayCutTypewriter();
        }
    }

    private void OnAnimatorFinished(ElementAnimator animator)
    {
        animator.onFinished -= OnAnimatorFinished;
        Debug.Assert(animator == currentAnimators[0]);
        currentAnimators.RemoveAt(0);
        if(currentAnimators.Count > 0)
        {
            StartAnimator(currentAnimators[0]);
        }
    }

    private void StartAnimator(ElementAnimator animator)
    {
        animator.onFinished += OnAnimatorFinished;
        animator.Start();
    }

    public void PrepareForDiaryScreenshot(DiaryEntryData entry)
    {
        if(entry != null)
        {
            GameObject newContentPageGO = Instantiate(contentPagePrefab, contentParent.transform);
            DiaryContentPage newContentPage = newContentPageGO.GetComponent<DiaryContentPage>();
            CreatePageContent(entry.entry.leftPage, newContentPage.LeftPage.transform, false);
            CreatePageContent(entry.entry.rightPage, newContentPage.RightPage.transform, false);
            screenshotPage = newContentPage;
        }

        nextPageButton.gameObject.SetActive(false);
        prevPageButton.gameObject.SetActive(false);
    }

    public void ResetFromScreenshot()
    {
        if(screenshotPage)
        {
            Destroy(screenshotPage);
        }

        UpdateButtons();
    }
}
