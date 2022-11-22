using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{

    public GameObject endDayPopup;
    public GameObject startDayHostelPopup;
    public GameObject startDayOutsidePopup;
    public GameObject foodDistributePopup;
    public GameObject endGamePopup;

    
    public ItemCategory foodCategory;
    private int foodAmount;
    public Text foodAmountText;
    private int moneyToBePaid = 10;
    public Text moneyText;

    public int motherFoodAmount=0;
    public int boyFoodAmount=0;
    public int girlFoodAmount=0;

    // Update is called once per frame
    void Update()
    {
        if (foodDistributePopup.activeSelf == true)
        {
            foodAmountText.text = foodAmount.ToString();
            moneyText.text = moneyToBePaid.ToString();
        }
    }

    public void OpenEndDayPopUp() 
    {
        endDayPopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void OpenStartDayHostelPopUp()
    {
        startDayHostelPopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void OpenStartDayOutsidePopUp()
    {
        startDayOutsidePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void OpenFoodDistributePopUp()
    {
        foodDistributePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);

        foodAmount = NewGameManager.Instance.inventory.GetItemCategoryCount(foodCategory);
        foodAmountText.text = foodAmount.ToString();
    }

    public void AddFood() 
    {
        foodAmount++;
        moneyToBePaid += 5;
    }

    public void DeductFood()
    {
        foodAmount--;
        moneyToBePaid -= 5;
    }

    public void SleepInHostelMethod() 
    {
        NewGameManager.Instance.SleepInHostel(moneyToBePaid, motherFoodAmount, boyFoodAmount, girlFoodAmount);
    }

    public void OpenEndGamePopUp()
    {
        endGamePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }
}
