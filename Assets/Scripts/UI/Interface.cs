using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InterfaceVisibilityFlags
{
    /// No interface elements are visible
    None = 0,
    /// Only the clock button is visible
    ClockButton = 1,
    /// Only the diary button is visible
    DiaryButton = 2,
    // If the status needs to be divided (handle e.g. food and money separately), add bitflags
    /// Only the status is visible
    StatusInfo = 4,

    /// All interface elements are visible
    All = ClockButton | DiaryButton | StatusInfo
}

public class Interface : MonoBehaviour
{
    [SerializeField]
    private GameObject locationInfo;
    [SerializeField]
    private Text foodText;
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Text locationText;
    [SerializeField]
    private Text dateText;
    [SerializeField]
    private GameObject clockButton;
    [SerializeField]
    private GameObject diaryButton;
    [SerializeField]
    private IngameDiary ingameDiary;
    [SerializeField]
    private PopupManager popupManager;

    public IngameDiary IngameDiary { get { return ingameDiary; } }

    private void Start()
    {
        NewGameManager.Instance.onDateChanged += OnDateChanged;
        NewGameManager.Instance.onFoodChanged += OnFoodChanged;
        NewGameManager.Instance.onMoneyChanged += OnMoneyChanged;

        OnMoneyChanged(NewGameManager.Instance.money);
        OnFoodChanged(NewGameManager.Instance.food);
        OnDateChanged(NewGameManager.Instance.date);
        OnLocationChanged(NewGameManager.Instance.currentLocation);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onDateChanged -= OnDateChanged;
            NewGameManager.Instance.onFoodChanged -= OnFoodChanged;
            NewGameManager.Instance.onMoneyChanged -= OnMoneyChanged;
        }
    }

    private void OnMoneyChanged(int money)
    {
        moneyText.text = money.ToString();
    }

    private void OnFoodChanged(int food)
    {
        foodText.text = food.ToString();
    }

    private void OnDateChanged(string date)
    {
        dateText.text = date;
    }

    private void OnLocationChanged(string location)
    {
        locationText.text = location;
    }

    /**
     * Hides every ui element like button or location text.
     */
    public void SetUIElementsVisible(InterfaceVisibilityFlags flags)
    {
        locationInfo.SetActive((flags & InterfaceVisibilityFlags.StatusInfo) != 0);
        clockButton.SetActive((flags & InterfaceVisibilityFlags.ClockButton) != 0);
        diaryButton.SetActive((flags & InterfaceVisibilityFlags.DiaryButton) != 0);
    }

    public void SetDiaryOpened(bool opened)
    {
        ingameDiary.SetOpened(opened);
    }

    public void SetDiaryOpened(DiaryPageLink page)
    {
        ingameDiary.SetOpened(page);
    }

    public void OpenDiaryImmediately(DiaryPageLink type)
    {
        ingameDiary.OpenImmediately(type);
    }

    public void CloseDiaryImmediately()
    {
        ingameDiary.CloseImmediately();
    }

    public void PrepareForMapScreenshot()
    {
        ///@todo
        //diary.PrepareForMapScreenshot();
    }

    public void PrepareForDiaryScreenshot(DiaryEntryData entry)
    {
        ///@todo
        //diary.PrepareForDiaryScreenshot(entry);
    }

    public void ResetFromScreenshot()
    {
        ///@todo
        //diary.ResetFromScreenshot();
    }

    public void OpenEndDayPopUp()
    {
        popupManager.OpenEndDayPopUp();
    }
}
