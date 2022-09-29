using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DiaryPagePrefabs
{
    public GameObject firstPage;
}

public class DiaryPages : MonoBehaviour
{
    [SerializeField]
    private GameObject contentLeft;
    [SerializeField]
    private GameObject contentRight;
    [SerializeField, Tooltip("True if new entries are allowed on the same double page, if the previous entry only requires the left page.")]
    private bool allowNewEntriesOnSameDoublePage = true;
    [SerializeField]
    private DiaryPagePrefabs prefabs;
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private Button nextPageButton;

    private List<GameObject> pages = new List<GameObject>();
    private int currentDoublePageIndex = -1;

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

    private void Start()
    {
        prevPageButton.onClick.AddListener(OpenPrevDoublePage);
        nextPageButton.onClick.AddListener(OpenNextDoublePage);
    }

    public void AddEntry(DiaryEntry entry)
    {
        // Add an empty page if the previous entry ended left, but the new one should also start left.
        bool isRight = allowNewEntriesOnSameDoublePage && !entry.startOnNewDoublePage && pages.Count % 2 != 0;
        if(!isRight && pages.Count % 2 != 0)
        {
            pages.Add(null);
        }

        int firstPageIndex = pages.Count;

        foreach(DiaryPageData data in entry.pages)
        {
            bool newPageIsLeft = pages.Count % 2 == 0;
            GameObject parent = newPageIsLeft ? contentLeft : contentRight;
            GameObject newPage = Instantiate(data.prefab, parent.transform);
            IDiaryPage diaryPage = newPage.GetComponent<IDiaryPage>();
            diaryPage.SetData(data);
            newPage.SetActive(false);
            pages.Add(newPage);
        }

        OpenDoublePage(GetDoublePageIndexFromPageIndex(firstPageIndex));
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
        if(currentDoublePageIndex > 0)
        {
            OpenDoublePage(currentDoublePageIndex - 1);
        }
    }

    public void OpenNextDoublePage()
    {
        if(currentDoublePageIndex < DoublePageCount - 1)
        {
            OpenDoublePage(currentDoublePageIndex + 1);
        }
    }
}
