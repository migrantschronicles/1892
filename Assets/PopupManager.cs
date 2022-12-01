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
    public float nightTime = 3f;



    public ItemCategory foodCategory;
    private int foodAmount;
    public Text foodAmountText;
    private int moneyToBePaid = 10;
    public Text moneyText;
    public Text stolenItemsText;
    public GameObject nonStolenItemsTXT; // Represents the text GO
    public GameObject stolenItemsTXT; // Represents the text GO

    public int motherFoodAmount=0;
    public int boyFoodAmount=0;
    public int girlFoodAmount=0;
    public int purchasedFoodAmount = 0;

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

    public void CloseAllPopups() 
    {
        endDayPopup.SetActive(false);
        startDayHostelPopup.SetActive(false);
        startDayOutsidePopup.SetActive(false);
        foodDistributePopup.SetActive(false);
        endGamePopup.SetActive(false);

        // Reset Food distribution 
        motherFood.SetActive(false);
        boyFood.SetActive(false);
        girlFood.SetActive(false);
        motherFoodAmount = 0;
        boyFoodAmount = 0;
        girlFoodAmount = 0;
        purchasedFoodAmount = 0;
    }

    public void OpenEndDayPopUp() 
    {
        CloseAllPopups();
        endDayPopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    public void OpenStartDayHostelPopUp()
    {
        CloseAllPopups();
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
        CloseAllPopups();
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
        CloseAllPopups();
        foodDistributePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);

        foodAmount = NewGameManager.Instance.inventory.GetItemCategoryCount(foodCategory);
        foodAmountText.text = foodAmount.ToString();
    }

    public void AddFood() 
    {
        foodAmount++;
        moneyToBePaid += 5;
        purchasedFoodAmount++;
    }

    public void DeductFood()
    {
        if (foodAmount != 0) { 
            foodAmount--;
            moneyToBePaid -= 5;
            purchasedFoodAmount--;
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
        NewGameManager.Instance.SleepInHostel(moneyToBePaid,  purchasedFoodAmount ,motherFoodAmount, boyFoodAmount, girlFoodAmount);
        StartCoroutine(SleepHostelTransition());
    }

    public void SleepOutsideMethod()
    {
        NewGameManager.Instance.SleepOutside();// motherFoodAmount, boyFoodAmount, girlFoodAmount);
        StartCoroutine(SleepOutsideTransition());
    }

    public void OpenEndGamePopUp()
    {
        endGamePopup.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
    }

    IEnumerator SleepOutsideTransition() 
    {
        nightTransition.SetActive(true);
        // Implement all changes needed based on Sleeping outside
        List<StolenItemInfo> stolenItems = NewGameManager.Instance.StealItems();
        NewGameManager.Instance.StartNewDay();
        CloseAllPopups();
        OpenStartDayOutsidePopUp();
        // Update stolen text info
        string stolenItemsStr = "";
        if (stolenItems.Count != 0)
        {
            for (int i = 0; i < stolenItems.Count; i++)
            {

                if (stolenItems[i].type == StolenItemType.Money)
                {
                    stolenItemsStr += stolenItems[i].money + "x francs";
                }
                else
                {
                    stolenItemsStr += LocalizationManager.Instance.GetLocalizedString(stolenItems[i].item.Name);
                }

                if (i != stolenItems.Count - 1) { stolenItemsStr += ", "; }

            }
            stolenItemsText.text = stolenItemsStr;

            stolenItemsTXT.SetActive(true);
            nonStolenItemsTXT.SetActive(false);
        }
        else 
        {
            stolenItemsTXT.SetActive(false);
            nonStolenItemsTXT.SetActive(true);
        }

        // Need to update clock to new time visually.
        //
        yield return new WaitForSeconds(nightTime);
        nightTransition.SetActive(false);
    }

    IEnumerator SleepHostelTransition()
    {
        nightTransition.SetActive(true);
        // Implement all changes needed based on Sleeping outside
        NewGameManager.Instance.StartNewDay();
        CloseAllPopups();
        OpenStartDayHostelPopUp();
        // Need to update clock to new time visually.
        //
        yield return new WaitForSeconds(nightTime);
        nightTransition.SetActive(false);
    }
}
