using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField]
    private InventoryContainer Basket;
    [SerializeField]
    private InventoryContainer Luggage;
    [SerializeField]
    private Button AcceptButton;
    [SerializeField]
    private Button CancelButton;
    [SerializeField]
    private GameObject arrowLeft;
    [SerializeField]
    private GameObject arrowRight;
    [SerializeField]
    private Item[] ShopItems;

    private bool transferInProgress = false;
    /// The changes during a transfer.
    /// Positive values mean from basket to luggage, negative values mean from luggage to basket.
    private Dictionary<Item, int> transferChanges = new Dictionary<Item, int>();

    private void Start()
    {
        Basket.OnSlotClicked.AddListener(OnBasketItemClicked);
        Luggage.OnSlotClicked.AddListener(OnLuggageItemClicked);
        arrowLeft.SetActive(false);
        arrowRight.SetActive(false);

        foreach(Item item in ShopItems)
        {
            if(item != null)
            {
                Basket.TryAddItem(item);
            }
        }
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

        UpdateArrows();
    }

    private void UpdateArrows()
    {
        bool hasPositiveValues = transferChanges.Values.Any((value) => value > 0);
        bool hasNegativeValues = transferChanges.Values.Any((value) => value < 0);
        arrowLeft.SetActive(hasNegativeValues);
        arrowRight.SetActive(hasPositiveValues);
    }

    private void ConditionallyStartTransfer()
    {
        if(!transferInProgress)
        {
            transferInProgress = true;
            Basket.EnableGhostMode();
            Luggage.EnableGhostMode();
            AcceptButton.gameObject.SetActive(true);
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
        UpdateArrows();
        transferInProgress = false;
    }

    private void AcceptTransfer()
    {
        Basket.ApplyGhostMode();
        Luggage.ApplyGhostMode();
        StopTransfer();
    }

    private void CancelTransfer()
    {
        Basket.CancelGhostMode();
        Luggage.CancelGhostMode();
        StopTransfer();
    }
}
