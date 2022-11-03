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
    private GameObject contentLeft;
    [SerializeField]
    private GameObject contentRight;
    [SerializeField, Tooltip("True if new entries are allowed on the same double page, if the previous entry only requires the left page.")]
    private bool allowNewEntriesOnSameDoublePage = true;
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private Button nextPageButton;
    [SerializeField]
    private Button contentLeftButton;
    [SerializeField]
    private Button contentRightButton;

    private List<GameObject> pages = new List<GameObject>();
    private int currentDoublePageIndex = -1;
    private List<ElementAnimator> currentAnimators = new List<ElementAnimator>();
    private DiaryEntry entryToCapture;

    /**
     * @return The number of double pages. If the last page ends on the left, it returns the same number as if the last page would end on the right.
     */
    public int DoublePageCount
    {
        get
        {
            return (pages.Count + 1) / 2;
        }
    }

    private void Awake()
    {
        prevPageButton.onClick.AddListener(OpenPrevDoublePage);
        nextPageButton.onClick.AddListener(OpenNextDoublePage);
        contentLeftButton.onClick.AddListener(() => StopAnimators(true));
        contentRightButton.onClick.AddListener(() => StopAnimators(true));
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

    public void OnVisiblityChanged(bool visible)
    {
        if (visible)
        {
            UpdateButtons();
        }
        else
        {
            StopAnimators(true);
        }
    }

    private void UpdateButtons()
    {
        prevPageButton.gameObject.SetActive(currentDoublePageIndex > 0);
        nextPageButton.gameObject.SetActive(currentDoublePageIndex < DoublePageCount - 1);
    }

    public void AddEntry(DiaryEntry entry)
    {
        StopAnimators();
        entryToCapture = entry;

        // Add an empty page if the previous entry ended left, but the new one should also start left.
        bool isRight = allowNewEntriesOnSameDoublePage && !entry.startOnNewDoublePage && pages.Count % 2 != 0;
        if(!isRight && pages.Count % 2 != 0)
        {
            pages.Add(null);
        }

        int firstPageIndex = pages.Count;
        ///@todo Change this to the real date. Localize this?
        string date = Time.time.ToString();

        foreach(DiaryPageData data in entry.pages)
        {
            data.Date = date;
            bool newPageIsLeft = pages.Count % 2 == 0;
            GameObject parent = newPageIsLeft ? contentLeft : contentRight;
            GameObject newPage = Instantiate(data.prefab, parent.transform);

            foreach(DiaryPageDrawing drawing in data.drawings)
            {
                if(drawing.IsEnabled)
                {
                    GameObject drawingGO = AddDrawingToPage(newPage, drawing);
                    currentAnimators.Add(ImageElementAnimator.FromImage(this, drawingGO.GetComponentInChildren<Image>()));
                }
            }

            IDiaryPage diaryPage = newPage.GetComponent<IDiaryPage>();
            diaryPage.SetData(data);
            currentAnimators.AddRange(diaryPage.CreateAnimators());

            newPage.SetActive(false);
            pages.Add(newPage);
        }

        OpenDoublePage(GetDoublePageIndexFromPageIndex(firstPageIndex));

        if(currentAnimators.Count > 0)
        {
            StartAnimator(currentAnimators[0]);
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

    public void OpenDoublePage(int index)
    {
        if(!IsDoublePageIndexValid(index))
        {
            return;
        }

        CloseCurrentPages();
        currentDoublePageIndex = index;
        SetDoublePageActive(currentDoublePageIndex, true);
        UpdateButtons();
    }

    private void CloseCurrentPages()
    {
        if(IsDoublePageIndexValid(currentDoublePageIndex))
        {
            SetDoublePageActive(currentDoublePageIndex, false);
            currentDoublePageIndex = -1;
        }
    }

    private void SetDoublePageActive(int index, bool active)
    {
        GameObject pageLeft = pages.ElementAtOrDefault(index * 2);
        pageLeft?.SetActive(active);
        GameObject pageRight = pages.ElementAtOrDefault(index * 2 + 1);
        pageRight?.SetActive(active);
    }

    private bool IsDoublePageIndexValid(int index)
    {
        return index >= 0 && index < DoublePageCount;
    }

    private int GetDoublePageIndexFromPageIndex(int index)
    {
        return index / 2;
    }

    public void OpenPrevDoublePage()
    {
        StopAnimators(true);
        
        if(currentDoublePageIndex > 0)
        {
            OpenDoublePage(currentDoublePageIndex - 1);
        }
    }

    public void OpenNextDoublePage()
    {
        StopAnimators(true);

        if(currentDoublePageIndex < DoublePageCount - 1)
        {
            OpenDoublePage(currentDoublePageIndex + 1);
        }
    }

    public void StopAnimators(bool takeScreenshot = false)
    {
        foreach(ElementAnimator animator in currentAnimators)
        {
            animator.Finish();
        }
        currentAnimators.Clear();
        if(takeScreenshot)
        {
            ConditionallyCaptureScreen();
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
        else
        {
            ConditionallyCaptureScreen();
        }
    }

    private void StartAnimator(ElementAnimator animator)
    {
        animator.onFinished += OnAnimatorFinished;
        animator.Start();
    }

    private void ConditionallyCaptureScreen()
    {
        if(!entryToCapture)
        {
            return;
        }

        LevelInstance.Instance.TakeDiaryEntryScreenshot();
        entryToCapture = null;
    }
}
