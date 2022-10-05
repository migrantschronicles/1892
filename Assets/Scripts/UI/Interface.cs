using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private Diary diary;

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
