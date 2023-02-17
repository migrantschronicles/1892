using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class PopupStackInfo
{
    public GameObject popupGO;
    public IPopup popup;
}

public class PopupManager : MonoBehaviour
{
    Stack<PopupStackInfo> popupStack = new Stack<PopupStackInfo>();

    public void PushPopup(GameObject popupGO)
    {
        if(popupStack.TryPeek(out PopupStackInfo prevInfo))
        {
            prevInfo.popup.RemoveOnCanCloseChangedListener(OnCanCloseChanged);
            prevInfo.popupGO.SetActive(false);
        }

        IPopup popup = popupGO.GetComponent<IPopup>();
        bool canClose = popup != null ? popup.CanClose : true;
        PopupStackInfo info = new()
        {
            popupGO = popupGO,
            popup = popup
        };
        popupStack.Push(info);
        popup.AddOnCanCloseChangedListener(OnCanCloseChanged);
        LevelInstance.Instance.SetBackButtonVisible(canClose);
    }

    /**
     * Removes the top popup from the stack and destroys it.
     * @return True if there are still more popups, false if this was the last popup.
     */
    public bool PopPopup()
    {
        if(popupStack.TryPop(out PopupStackInfo info))
        {
            Destroy(info.popupGO);
        }

        if(popupStack.TryPeek(out PopupStackInfo nextInfo))
        {
            nextInfo.popupGO.SetActive(true);
            nextInfo.popup.AddOnCanCloseChangedListener(OnCanCloseChanged);
            LevelInstance.Instance.SetBackButtonVisible(nextInfo.popup.CanClose);
            return true;
        }

        return false;
    }

    /**
     * Deletes the stack and destroys all popups.
     */
    public void ClearHistory()
    {
        while(popupStack.TryPop(out PopupStackInfo info))
        {
            Destroy(info.popupGO);
        }
    }

    private void OnCanCloseChanged(IPopup popup, bool canClose)
    {
        if(popupStack.TryPeek(out PopupStackInfo info))
        {
            if(info.popup == popup)
            {
                LevelInstance.Instance.SetBackButtonVisible(canClose);
            }
        }
    }

    /*
    public GameObject endDayPopup;
    public GameObject startDayHostelPopup;
    public GameObject startDayOutsidePopup;
    public GameObject foodDistributePopupHostel;
    public GameObject foodDistributePopupOutside;
    public GameObject endGamePopup;
    public GameObject backBTNEndDay;
    public GameObject nightTransition;
    public float nightTime = 3f;

    



    public ItemCategory foodCategory;
    private int foodAmount;
    public Text foodAmountTextHostel;
    public Text foodAmountTextOutside;
    private int moneyToBePaid = 0;
    public Text moneyText;
    public Text stolenItemsText;
    public GameObject nonStolenItemsTXT; // Represents the text GO
    public GameObject stolenItemsTXT; // Represents the text GO

    public int motherFoodAmount=0;
    public int boyFoodAmount=0;
    public int girlFoodAmount=0;
    public int purchasedFoodAmount = 0;


    public int hostelMoneyEU = 2;
    public int hostelMoneyUS = 1;
    public Button stayInHostelBTN;

    //public GameObject motherFoodHostel; // Food icon for mother in Hostel
    public Text motherFoodTXTHostel;
    public Text motherFoodTXTOutside;
    public GameObject motherFoodBoxHostel;
    //public GameObject boyFoodHostel; // Food icon for boy in Hostel
    public Text boyFoodTXTHostel;
    public Text boyFoodTXTOutside;
    public GameObject boyFoodBoxHostel;
    //public GameObject girlFoodHostel; // Food icon for girl in Hostel
    public Text girlFoodTXTHostel;
    public Text girlFoodTXTOutside;
    public GameObject girlFoodBoxHostel;
    //public GameObject motherFoodOutside; // Food icon for mother in Outside
    public GameObject motherFoodBoxOutside;
    //public GameObject boyFoodOutside; // Food icon for boy in Outside
    public GameObject boyFoodBoxOutside;
    //public GameObject girlFoodOutside; // Food icon for girl in Outside
    public GameObject girlFoodBoxOutside;
    public GameObject deductFoodBTN;
    public GameObject addFoodBTN;
    public Color hasFoodColor;
    public Color noFoodColor;



    // Update is called once per frame
    void Update()
    {
        if(endDayPopup.activeSelf == true && NewGameManager.Instance.hour < NewGameManager.Instance.hoursPerDay) 
        {
            backBTNEndDay.SetActive(true);
        }
        else { backBTNEndDay.SetActive(false); }

        if (foodDistributePopupHostel.activeSelf == true)
        {
            foodAmountTextHostel.text = foodAmount.ToString();
            moneyText.text = moneyToBePaid.ToString();
        }

        if (foodDistributePopupOutside.activeSelf == true)
        {
            foodAmountTextOutside.text = foodAmount.ToString();
        }
    }

    public void CloseAllPopups() 
    {
        backBTNEndDay.SetActive(false);
        endDayPopup.SetActive(false);
        startDayHostelPopup.SetActive(false);
        startDayOutsidePopup.SetActive(false);
        foodDistributePopupHostel.SetActive(false);
        foodDistributePopupOutside.SetActive(false);
        endGamePopup.SetActive(false);

        // Reset Food distribution 

        
        motherFoodTXTHostel.text = "0";
        motherFoodTXTOutside.text = "0";
        boyFoodTXTHostel.text = "0";
        boyFoodTXTOutside.text = "0";
        girlFoodTXTHostel.text = "0";
        girlFoodTXTOutside.text = "0";

        motherFoodAmount = 0;
        boyFoodAmount = 0;
        girlFoodAmount = 0;
        purchasedFoodAmount = 0;
    }

    public void OpenEndDayPopUp() 
    {
        CloseAllPopups();
        endDayPopup.SetActive(true);

        // Should have a case for US money as well
        if (NewGameManager.Instance.money >= hostelMoneyEU) stayInHostelBTN.interactable = true;
        else stayInHostelBTN.interactable = false;

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

    public void OpenFoodDistributePopUpHostel()
    {
        CloseAllPopups();

        motherFoodBoxHostel.GetComponent<Image>().color = noFoodColor;
        boyFoodBoxHostel.GetComponent<Image>().color = noFoodColor;
        girlFoodBoxHostel.GetComponent<Image>().color = noFoodColor;

        foodDistributePopupHostel.SetActive(true);
        NewGameManager.Instance.SetPaused(true);

        if (NewGameManager.Instance.CurrentCurrency == Currency.Franc) // Do the same for the button text from end day pop-up.
            moneyToBePaid = hostelMoneyEU;
        else
            moneyToBePaid = hostelMoneyUS;

        foodAmount = NewGameManager.Instance.inventory.GetItemCategoryCount(foodCategory);
        foodAmountTextHostel.text = foodAmount.ToString();

        // Auto-assign food based on needs.
        DistributeFood();

    }

    public void OpenFoodDistributePopUpOutside()
    {
        CloseAllPopups();

        motherFoodBoxOutside.GetComponent<Image>().color = noFoodColor;
        boyFoodBoxOutside.GetComponent<Image>().color = noFoodColor;
        girlFoodBoxOutside.GetComponent<Image>().color = noFoodColor;

        foodDistributePopupOutside.SetActive(true);
        NewGameManager.Instance.SetPaused(true);
        moneyToBePaid = 0;
        foodAmount = NewGameManager.Instance.inventory.GetItemCategoryCount(foodCategory);
        foodAmountTextOutside.text = foodAmount.ToString();

        // Auto-assign food based on needs.
        DistributeFood();
    }

    private void DistributeFood() // Based on who is more hungry.
    {
        for(int i=0;i<foodAmount; i++) 
        {
            DistributeFoodItem();
        }
    }

    public void DistributeFoodItem() // Individually
    {
        int maxFoodRequired = 0;
        ProtagonistHealthData characterInNeed = null;

        foreach (ProtagonistHealthData characterHealth in NewGameManager.Instance.HealthStatus.Characters)
        {
            int tempNextRequiredFoodAmount = characterHealth.HungryStatus.NextRequiredFoodAmount;

            switch (characterHealth.CharacterData.name) 
            {
                case "Elis":
                    tempNextRequiredFoodAmount -= motherFoodAmount;
                    break;

                case "Mattis":
                    tempNextRequiredFoodAmount -= boyFoodAmount;
                    break;

                case "Mreis":
                    tempNextRequiredFoodAmount -= girlFoodAmount;
                    break;
            }

            if (tempNextRequiredFoodAmount > maxFoodRequired)
            {
                maxFoodRequired = tempNextRequiredFoodAmount;
                characterInNeed = characterHealth;
            }
        }

        if (maxFoodRequired > 0)
        {
            if (characterInNeed.CharacterData.name == "Elis")
                FeedMother();

            else if (characterInNeed.CharacterData.name == "Mattis")
                FeedBoy();

            else if (characterInNeed.CharacterData.name == "Mreis")
                FeedGirl();
        }
        //else if(maxFoodRequired == 0) 
    }

    public void AddFood() 
    {
        foodAmount++;
        moneyToBePaid += 5;
        purchasedFoodAmount++;

        DistributeFood();
    }

    public void DeductFood() // Should remove deduct food button.
    {

    }

    public void FeedMother() 
    {
        
        if(foodAmount>0) { 
            //motherFoodOutside.SetActive(true);
            motherFoodBoxHostel.GetComponent<Image>().color = hasFoodColor;
            motherFoodBoxOutside.GetComponent<Image>().color = hasFoodColor;
            motherFoodAmount++;
            foodAmount--;
            foodAmountTextHostel.text = foodAmount.ToString();
            foodAmountTextOutside.text = foodAmount.ToString();
            motherFoodTXTHostel.text = motherFoodAmount.ToString();
            motherFoodTXTOutside.text = motherFoodAmount.ToString();
            // Need to add one for outside

        }
    }

    public void FeedBoy()
    {
        if (foodAmount > 0)
        {
            //boyFoodOutside.SetActive(true);
            boyFoodBoxHostel.GetComponent<Image>().color = hasFoodColor;
            boyFoodBoxOutside.GetComponent<Image>().color = hasFoodColor;
            boyFoodAmount++;
            foodAmount--;
            foodAmountTextHostel.text = foodAmount.ToString();
            foodAmountTextOutside.text = foodAmount.ToString();
            boyFoodTXTHostel.text = boyFoodAmount.ToString();
            boyFoodTXTOutside.text = boyFoodAmount.ToString();
        }
    }

    public void FeedGirl()
    {
        if (foodAmount > 0)
        {
            //girlFoodOutside.SetActive(true);
            girlFoodBoxHostel.GetComponent<Image>().color = hasFoodColor;
            girlFoodBoxOutside.GetComponent<Image>().color = hasFoodColor;
            girlFoodAmount++;
            foodAmount--;
            foodAmountTextHostel.text = foodAmount.ToString();
            foodAmountTextOutside.text = foodAmount.ToString();
            girlFoodTXTHostel.text = girlFoodAmount.ToString();
            girlFoodTXTOutside.text = girlFoodAmount.ToString();
        }
    }


    public void SleepInHostelMethod() 
    {
        NewGameManager.Instance.SleepInHostel(moneyToBePaid,  purchasedFoodAmount ,motherFoodAmount, boyFoodAmount, girlFoodAmount);
        StartCoroutine(SleepHostelTransition());
    }

    public void SleepOutsideMethod()
    {
        NewGameManager.Instance.SleepOutside(motherFoodAmount, boyFoodAmount, girlFoodAmount);
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
    */
}
