using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField]
    private ScrollableInventoryManager Basket;
    [SerializeField]
    private ScrollableInventoryManager Luggage;
    [SerializeField]
    private Button AcceptButton;
    [SerializeField]
    private Button CancelButton;
    [SerializeField]
    private GameObject arrowLeft;
    [SerializeField]
    private GameObject arrowRight;
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Item[] ShopItems;

    private bool transferInProgress = false;
    /// The changes during a transfer.
    /// Positive values mean from basket to luggage, negative values mean from luggage to basket.
    private Dictionary<Item, int> transferChanges = new Dictionary<Item, int>();

    private void Start()
    {
        Basket.onSlotClicked.AddListener(OnBasketItemClicked);
        Luggage.onSlotClicked.AddListener(OnLuggageItemClicked);
        arrowLeft.SetActive(false);
        arrowRight.SetActive(false);

        Basket.SetBagCount(1);
        Luggage.SetBagCount(3);

        Basket.ResetItems(ShopItems);
        Luggage.ResetItems(NewGameManager.Instance.inventory.Items);

        Luggage.onItemAmountChanged += OnLuggageItemAmountChanged;
    }

    private void OnBasketItemClicked(InventorySlot slot)
    {
        ConditionallyStartTransfer();
        if(Luggage.TryAddItem(slot.Item))
        {
            if(!Basket.TryRemoveItemAt(slot.X, slot.Y))
            {
                Debug.Log("Item added to luggage could not be removed from basket");
            }

            LogTransferChange(slot.Item, 1);
        }
    }

    private void OnLuggageItemClicked(InventorySlot slot)
    {
        ConditionallyStartTransfer();
        if(Basket.TryAddItem(slot.Item))
        {
            if(!Luggage.TryRemoveItemAt(slot.X, slot.Y))
            {
                Debug.Log("Item added to basket could not be removed from luggage");
            }

            LogTransferChange(slot.Item, -1);
        }
    }

    private void LogTransferChange(Item item, int amount)
    {
        if(transferChanges.TryGetValue(item, out int value))
        {
            int newValue = value + amount;
            if(newValue == 0)
            {
                transferChanges.Remove(item);
            }
            else
            {
                transferChanges[item] = newValue;
            }
        }
        else
        {
            transferChanges.Add(item, amount);
        }

        UpdateDynamics();
    }

    private void UpdateDynamics()
    {
        // Arrows
        bool hasPositiveValues = transferChanges.Values.Any((value) => value > 0);
        bool hasNegativeValues = transferChanges.Values.Any((value) => value < 0);
        arrowLeft.SetActive(hasNegativeValues);
        arrowRight.SetActive(hasPositiveValues);

        // Money
        int price = CalculatePrice();
        moneyText.text = price.ToString();
        moneyText.gameObject.SetActive(price != 0);

        // Accept Button
        AcceptButton.enabled = price < 0 || NewGameManager.Instance.money >= price;

        // Back button
        LevelInstance.Instance.SetBackButtonVisible(transferChanges.Count == 0);
    }

    private int CalculatePrice()
    {
        int price = 0;
        foreach(var item in transferChanges)
        {
            price += item.Key.Price * item.Value;
        }
        return price;
    }

    private void ConditionallyStartTransfer()
    {
        if(!transferInProgress)
        {
            transferInProgress = true;
            Basket.EnableGhostMode();
            Luggage.EnableGhostMode();
            AcceptButton.gameObject.SetActive(true);
            AcceptButton.enabled = true;
            CancelButton.gameObject.SetActive(true);
            AcceptButton.onClick.AddListener(AcceptTransfer);
            CancelButton.onClick.AddListener(CancelTransfer);
        }
    }

    private void StopTransfer()
    {
        AcceptButton.onClick.RemoveListener(AcceptTransfer);
        CancelButton.onClick.RemoveListener(CancelTransfer);
        AcceptButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        transferChanges.Clear();
        UpdateDynamics();
        transferInProgress = false;
    }

    private void AcceptTransfer()
    {
        int price = CalculatePrice();
        if(price > 0 && price > NewGameManager.Instance.money)
        {
            return;
        }

        Basket.ApplyGhostMode();
        Luggage.ApplyGhostMode();
        StopTransfer();
        NewGameManager.Instance.SetMoney(NewGameManager.Instance.money - price);
    }

    private void CancelTransfer()
    {
        Basket.CancelGhostMode();
        Luggage.CancelGhostMode();
        StopTransfer();
    }

    private void OnLuggageItemAmountChanged(Item item, int changedAmount)
    {
        NewGameManager.Instance.inventory.OnItemAmountChanged(item, changedAmount);
    }
}
