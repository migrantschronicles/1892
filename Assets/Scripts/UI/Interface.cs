using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interface : MonoBehaviour
{
    [SerializeField]
    private GameObject locationInfo;
    [SerializeField]
    private GameObject clockButton;
    [SerializeField]
    private GameObject diaryButton;
    [SerializeField]
    private Diary diary;

    /**
     * Hides every ui element like button or location text.
     */
    public void SetUIElementsVisible(bool visible)
    {
        locationInfo.SetActive(visible);
        clockButton.SetActive(visible);
        diaryButton.SetActive(visible);
    }

    public void SetDiaryVisible(bool visible, DiaryPageType type = DiaryPageType.Inventory)
    {
        diary.SetVisible(visible, type);
    }
}
