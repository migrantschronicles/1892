using System.Collections;
using System.Collections.Generic;
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
    private Item[] ShopItems;

    private bool transferInProgress = false;

    private void Start()
    {
        Basket.OnSlotClicked.AddListener(OnBasketItemClicked);
        Luggage.OnSlotClicked.AddListener(OnLuggageItemClicked);

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
            ///@todo remove from basket 1 amount
        }
    }

    private void OnLuggageItemClicked(InventorySlot slot)
    {
        ConditionallyStartTransfer();
        if(Basket.TryAddItem(slot.Item))
        {
            ///@todo remove from luggage.
        }
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
