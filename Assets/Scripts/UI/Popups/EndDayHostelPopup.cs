using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndDayHostelPopup : EndDaySleepPopupBase
{
    [SerializeField]
    private Text priceText;
    [SerializeField]
    private int price;
    [SerializeField]
    private int hostelFee = 2;
    [SerializeField]
    private int foodCost = 5;
    [SerializeField]
    private int maxFoodAmount = 9;

    private int boughtFoodAmount = 0;

    public int HostelFee { get { return hostelFee; } }

    public int CurrentCost
    {
        get
        {
            return hostelFee + boughtFoodAmount * foodCost;
        }
    }

    private void Start()
    {
        CreatePortraits();
        InitFoodCounter();
        foodCounter.OnAdd += OnAdd;
        foodCounter.OnSubtract += OnSubtract;
        UpdateCosts();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateElements();
    }
#endif

    private void UpdateElements()
    {
        priceText.text = (-price).ToString();
    }

    private void OnSubtract()
    {
        if(foodCounter.Count <= 0 || foodCounter.Count <= (inventoryFoodCount - distributedFoodCount))
        {
            return;
        }

        --boughtFoodAmount;
        --foodCounter.Count;
        UpdateCosts();
    }

    private void OnAdd()
    {
        if(foodCounter.Count >= maxFoodAmount)
        {
            // Bought enough food.
            return;
        }

        if(CurrentCost + foodCost > NewGameManager.Instance.money)
        {
            // Too little money
            return;
        }

        ///@todo Change if food can be stackable or can have volume 2
        // Calculate if there is an available slot after distributed food and bought food gets added.
        // Assumes that food can't be stackable and can only have a volume of 1.
        int availableSlotCount = NewGameManager.Instance.inventory.GetAvailableSlotCount();
        int distributedFromInventory = Mathf.Min(distributedFoodCount, inventoryFoodCount);
        int distributedFromBought = distributedFoodCount - distributedFromInventory;
        if(availableSlotCount + distributedFromInventory - (boughtFoodAmount - distributedFromBought) <= 0)
        {
            // Inventory is full.
            return;
        }

        ++boughtFoodAmount;
        ++foodCounter.Count;
        UpdateCosts();
    }

    private void UpdateCosts()
    {
        price = CurrentCost;
        UpdateElements();
    }

    public void OnAccept()
    {
        LevelInstance.Instance.OnSleepInHostel(GetEndOfDayHealthData(), CurrentCost, boughtFoodAmount);
    }
}
