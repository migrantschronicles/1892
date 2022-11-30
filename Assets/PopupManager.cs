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
    public GameObject nightTransition;


    
    public ItemCategory foodCategory;
    private int foodAmount;
    public Text foodAmountText;
    private int moneyToBePaid = 10;
    public Text moneyText;

    public int motherFoodAmount=0;
    public int boyFoodAmount=0;
    public int girlFoodAmount=0;

    public GameObject motherFood;
    public GameObject boyFood;
    public GameObject girlFood;
    public GameObject deductFoodBTN;
    public GameObject addFoodBTN;


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

    public void CloseStartDayHostelPopUp()
    {
        startDayHostelPopup.SetActive(false);
        NewGameManager.Instance.SetPaused(false);
    }

    public void OpenStartDayOutsidePopUp()
    {
        startDayOutsidePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void CloseStartDayOutsidePopUp()
    {
        startDayOutsidePopup.SetActive(false);
        NewGameManager.Instance.SetPaused(false);
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
        if (foodAmount != 0) { 
            foodAmount--;
            moneyToBePaid -= 5;
        }
    }

    public void FeedMother() 
    {
        
        if(foodAmount>0 && motherFoodAmount == 0) { 
            motherFood.SetActive(true);
            motherFoodAmount++;
            foodAmount--;
            foodAmountText.text = foodAmount.ToString();
        }
        else if(motherFoodAmount != 0)
        {
            motherFood.SetActive(false);
            motherFoodAmount--;
            foodAmount++;
            foodAmountText.text = foodAmount.ToString();
        }
    }

    public void FeedBoy()
    {
        if (foodAmount > 0 && boyFoodAmount == 0)
        {
            boyFood.SetActive(true);
            boyFoodAmount++;
            foodAmount--;
            foodAmountText.text = foodAmount.ToString();
        }
        else if(boyFoodAmount != 0)
        {
            boyFood.SetActive(false);
            boyFoodAmount--;
            foodAmount++;
            foodAmountText.text = foodAmount.ToString();
        }
    }

    public void FeedGirl()
    {
        if (foodAmount > 0 && girlFoodAmount == 0)
        {
            girlFood.SetActive(true);
            girlFoodAmount++;
            foodAmount--;
            foodAmountText.text = foodAmount.ToString();
        }
        else if (girlFoodAmount != 0)
        {
            girlFood.SetActive(false);
            girlFoodAmount--;
            foodAmount++;
            foodAmountText.text = foodAmount.ToString();
        }
    }


    public void SleepInHostelMethod() 
    {
        NewGameManager.Instance.SleepInHostel(moneyToBePaid, motherFoodAmount, boyFoodAmount, girlFoodAmount);
        SleepFadeIn();
    }

    public void SleepOutsideMethod()
    {
        NewGameManager.Instance.SleepOutside();// motherFoodAmount, boyFoodAmount, girlFoodAmount);
        SleepFadeIn();
    }

    public void OpenEndGamePopUp()
    {
        endGamePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void SleepFadeIn() 
    {
        nightTransition.SetActive(true);
        /*while (nightTransition.GetComponent<Image>().color.a != 255) 
        {
            nightTransition.GetComponent<Image>().color.a += 0.01f;
        }*/

    }
}
