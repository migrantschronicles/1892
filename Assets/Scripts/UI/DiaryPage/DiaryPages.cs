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

    public void AddEntry(DiaryEntry entry)
    {
        bool rightPage = allowNewEntriesOnSameDoublePage && !entry.startOnNewDoublePage && pages.Count % 2 != 0;
        GameObject parent = rightPage ? contentRight : contentLeft;
        if(!rightPage && pages.Count % 2 != 0)
        {
            pages.Add(null);
        }

        GameObject newPage = Instantiate(prefabs.firstPage, parent.transform);
        FirstPage firstPage = newPage.GetComponent<FirstPage>();
        string text = LocalizationManager.Instance.GetLocalizedString(entry.text);
        text = firstPage.SetText(text);
        firstPage.SetDate(Time.time.ToString());
        newPage.SetActive(false);

        int doublePageIndex = pages.Count / 2;
        pages.Add(newPage);
        OpenDoublePage(doublePageIndex);
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
}
