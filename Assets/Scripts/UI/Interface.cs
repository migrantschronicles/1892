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
    private Text locationTextMapUI;
    [SerializeField]
    private Text dateText;
    [SerializeField]
    private GameObject clockButton;
    [SerializeField]
    private GameObject diaryButton;
    [SerializeField]
    private GameObject diaryBackground;
    [SerializeField]
    private IngameDiary ingameDiary;
    [SerializeField]
    private Image actionCounterBackground;
    [SerializeField]
    private Text actionCounter;
    [SerializeField]
    private Sprite actionCounterMalusBackground;
    [SerializeField]
    private Image infiniteActionCounter;
    [SerializeField]
    private Color actionCounterMalusForeground = Color.white;

    public IngameDiary IngameDiary { get { return ingameDiary; } }
    private bool TreatDiaryAsButton { get { return IngameDiary.Diary.Status == OpenStatus.Closed; } }

    private InterfaceVisibilityFlags visibilityFlags = InterfaceVisibilityFlags.All;
    private Sprite defaultActionCounterBackground;
    private Color defaultActionCounterForeground;

    public InterfaceVisibilityFlags VisibilityFlags { get { return visibilityFlags; } }

    private void Awake()
    {
        ingameDiary.gameObject.SetActive(true);
        ingameDiary.Diary.onDiaryStatusChanged += (status) =>
        {
            if(status == OpenStatus.Closed)
            {
                ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
                diaryBackground.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
            }
        };

        defaultActionCounterBackground = actionCounterBackground.sprite;
        defaultActionCounterForeground = actionCounter.color;
    }

    private void Start()
    {
        NewGameManager.Instance.onDateChanged += OnDateChanged;
        NewGameManager.Instance.onMoneyChanged += OnMoneyChanged;
        NewGameManager.Instance.inventory.onItemAmountChanged += OnItemAmountChanged;

        if(LevelInstance.Instance.MaxDialogsPerDay >= 0)
        {
            LevelInstance.Instance.OnDialogsTodayChanged += OnDialogsTodayChanged;
        }

        switch(LevelInstance.Instance.LevelMode)
        {
            case LevelInstanceMode.Default: 
                OnLocationChanged(LevelInstance.Instance.LocationName); 
                break;

            case LevelInstanceMode.Ship:
                LevelInstance.Instance.onCurrentRoomChanged += OnCurrentRoomChanged;
                OnCurrentRoomChanged(LevelInstance.Instance.CurrentRoom);
                locationTextMapUI.text = NewGameManager.Instance.LocationManager.GetLocalizedName("Ship");
                break;
        }

        OnMoneyChanged(NewGameManager.Instance.money);
        OnDateChanged(NewGameManager.Instance.date);
        foodText.text = NewGameManager.Instance.inventory.GetItemTypeCount(ItemType.Food).ToString();
        InitDialogsToday(LevelInstance.Instance.MaxDialogsPerDay);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onDateChanged -= OnDateChanged;
            NewGameManager.Instance.onMoneyChanged -= OnMoneyChanged;
            NewGameManager.Instance.inventory.onItemAmountChanged -= OnItemAmountChanged;
        }
    }

    private void OnMoneyChanged(int money)
    {
        moneyText.text = money.ToString();
    }

    private void OnDateChanged(DateTime date)
    {
        string dateStr = date.ToString("d MMMM yyyy");
        dateText.text = dateStr;
    }

    private void OnLocationChanged(string location)
    {
        locationText.text = NewGameManager.Instance.LocationManager.GetLocalizedName(location);
        locationTextMapUI.text = NewGameManager.Instance.LocationManager.GetLocalizedName(location);
    }

    private void OnCurrentRoomChanged(Room room)
    {
        if(room)
        {
            locationText.text = LocalizationManager.Instance.GetLocalizedString(room.RoomName);
        }
        else
        {
            locationText.text = NewGameManager.Instance.LocationManager.GetLocalizedName("Ship");
        }
    }

    private void OnItemAmountChanged(Item item, int changedAmount, int totalAmount)
    {
        if(item.category && item.category.type == ItemType.Food)
        {
            int totalFood = NewGameManager.Instance.inventory.GetItemTypeCount(ItemType.Food);
            foodText.text = totalFood.ToString();
        }
    }

    private void OnDialogsTodayChanged(int amount)
    {
        int remaining = LevelInstance.Instance.MaxDialogsPerDay - amount;
        actionCounter.text = remaining.ToString();
        actionCounterBackground.sprite = remaining > 0 ? defaultActionCounterBackground : actionCounterMalusBackground;
        actionCounter.color = remaining > 0 ? defaultActionCounterForeground : actionCounterMalusForeground;
    }

    public void InitDialogsToday(int amount)
    {
        actionCounter.gameObject.SetActive(amount >= 0);
        infiniteActionCounter.gameObject.SetActive(amount < 0);
    }

    /**
     * Hides every ui element like button or location text.
     */
    public void SetUIElementsVisible(InterfaceVisibilityFlags flags)
    {
        visibilityFlags = flags;
        locationInfo.SetActive((visibilityFlags & InterfaceVisibilityFlags.StatusInfo) != 0);
        clockButton.SetActive((visibilityFlags & InterfaceVisibilityFlags.ClockButton) != 0);
        diaryButton.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);

        if(TreatDiaryAsButton)
        {
            ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
            diaryBackground.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
        }
        else
        {
            diaryBackground.SetActive(false);
        }
    }

    public void SetDiaryOpened(bool opened)
    {
        if(opened)
        {
            ingameDiary.gameObject.SetActive(true);
            diaryBackground.SetActive(false);
        }
        ingameDiary.SetOpened(opened);
    }

    public void SetDiaryOpened(DiaryPageLink page)
    {
        ingameDiary.gameObject.SetActive(true);
        diaryBackground.SetActive(false);
        ingameDiary.SetOpened(page);
    }

    public void OpenDiaryImmediately(DiaryPageLink type)
    {
        ingameDiary.gameObject.SetActive(true);
        diaryBackground.SetActive(false);
        ingameDiary.OpenImmediately(type);
    }

    public void CloseDiaryImmediately()
    {
        ingameDiary.CloseImmediately();
        ingameDiary.gameObject.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
        diaryBackground.SetActive((visibilityFlags & InterfaceVisibilityFlags.DiaryButton) != 0);
    }

    public void HideDiary(bool hide)
    {
        ingameDiary.gameObject.SetActive(!hide);
        diaryBackground.SetActive(!hide);
    }
}
