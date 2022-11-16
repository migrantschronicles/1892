using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{

    private NewGameManager gameManager;
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

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<NewGameManager>();
        
    }

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
        gameManager.gameRunning = false;
    }

    public void OpenStartDayHostelPopUp()
    {
        startDayHostelPopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenStartDayOutsidePopUp()
    {
        startDayOutsidePopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenFoodDistributePopUp()
    {
        foodDistributePopup.SetActive(true);
        gameManager.gameRunning = false;

        foodAmount = gameManager.inventory.GetItemCategoryCount(foodCategory);
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
        gameManager.SleepInHostel(moneyToBePaid, motherFoodAmount, boyFoodAmount, girlFoodAmount);
    }

    public void OpenEndGamePopUp()
    {
        endGamePopup.SetActive(true);
        gameManager.gameRunning = false;
    }


}
