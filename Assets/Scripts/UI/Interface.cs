using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;

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
    private IngameDiary ingameDiary;
    [SerializeField]
    private PopupManager popupManager;

    public IngameDiary IngameDiary { get { return ingameDiary; } }
    private bool TreatDiaryAsButton { get { return IngameDiary.Diary.Status == OpenStatus.Closed; } }

    private InterfaceVisibilityFlags visibilityFlags = InterfaceVisibilityFlags.All;

    private void Awake()
    {
        ingameDiary.gameObject.SetActive(true);
        ingameDiary.Diary.onDiaryStatusChanged += (status) =>
        {
            if(status == OpenStatus.Closed)
            {
                ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
            }
        };
    }

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

    private void OnDateChanged(DateTime date)
    {
        DateTimeFormatInfo dateFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        string dateStr = date.ToString(dateFormat.ShortDatePattern);
        dateText.text = dateStr;
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
        visibilityFlags = flags;
        locationInfo.SetActive((visibilityFlags & InterfaceVisibilityFlags.StatusInfo) != 0);
        clockButton.SetActive((visibilityFlags & InterfaceVisibilityFlags.ClockButton) != 0);

        if(TreatDiaryAsButton)
        {
            ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
        }
    }

    public void SetDiaryOpened(bool opened)
    {
        if(opened)
        {
            ingameDiary.gameObject.SetActive(true);
        }
        ingameDiary.SetOpened(opened);
    }

    public void SetDiaryOpened(DiaryPageLink page)
    {
        ingameDiary.gameObject.SetActive(true);
        ingameDiary.SetOpened(page);
    }

    public void OpenDiaryImmediately(DiaryPageLink type)
    {
        ingameDiary.gameObject.SetActive(true);
        ingameDiary.OpenImmediately(type);
    }

    public void CloseDiaryImmediately()
    {
        ingameDiary.CloseImmediately();
        ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
    }

    public void PrepareForMapScreenshot()
    {
        ingameDiary.PrepareForMapScreenshot();
    }

    public void PrepareForDiaryScreenshot(DiaryEntryData entry)
    {
        ingameDiary.PrepareForDiaryScreenshot(entry);
    }

    public void ResetFromScreenshot()
    {
        ingameDiary.ResetFromScreenshot();
    }

    public void OpenEndDayPopUp()
    {
        popupManager.OpenEndDayPopUp();
    }
}
