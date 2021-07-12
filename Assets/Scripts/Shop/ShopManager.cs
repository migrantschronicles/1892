using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class ShopData
{
    public static List<int> ShopItemIds = new List<int> { 9,10,11,12 };
}

public class ShopManager : MonoBehaviour
{
    public List<InventorySlot> shopSlots = new List<InventorySlot>(12);
    public List<InventorySlot> luggageSlots = new List<InventorySlot>(12);

    public int moneyChanges = 0;
    public List<InventorySlot> shopAdditions;
    public List<InventorySlot> luggageAdditions;

    [Space(20)]
    public GameObject moneyChangesUI;
    public Text pendingMoneyChangeText;
    public Text moneyText;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject backButton;

    void Start()
    {
        foreach (var id in StateManager.CurrentState.AvailableItemIds)
        {
            foreach(var slot in luggageSlots)
            {
                if(slot.IsEmpty)
                {
                    slot.IsEmpty = false;
                    slot.ItemId = id;
                    slot.Location = "Luggage";
                    slot.ItemOriginalLocation = "Luggage";
                    slot.Value = InventoryData.InventoryById[id].Price;
                    slot.IconKey = InventoryData.InventoryById[id].Name;
                    break;
                }
            }
        }

        foreach (var id in ShopData.ShopItemIds)
        {
            foreach (var slot in shopSlots)
            {
                if(slot.IsEmpty)
                {
                    slot.ItemId = id;
                    slot.Location = "Shop";
                    slot.ItemOriginalLocation = "Shop";
                    slot.Value = InventoryData.InventoryById[id].Price;
                    slot.IconKey = InventoryData.InventoryById[id].Name;
                    break;
                }
            }
        }

        CheckStorage();
    }

    public void PickItem(InventorySlot item) 
    {
        if (!item.IsEmpty)
        {
            backButton.SetActive(false);
            moneyChangesUI.SetActive(true);

            InventorySlot updatedSlot = TransferItem(item);

            if (updatedSlot.Location == "Shop")
            {
                luggageAdditions.Add(updatedSlot);
            }
            else
            {
                shopAdditions.Add(updatedSlot);
            }
        }
    }

    public void AcceptChanges() 
    {
        if(StateManager.CurrentState.AvailableMoney + moneyChanges < 0)
        {
            return;
        }

        StateManager.CurrentState.AvailableMoney += moneyChanges;
        StateManager.CurrentState.AvailableItemIds = luggageSlots.Where(s => !s.IsEmpty).Select(s => s.ItemId.Value);
        ShopData.ShopItemIds = shopSlots.Where(s => !s.IsEmpty).Select(s => s.ItemId.Value).ToList();

        moneyChanges = 0;
        moneyChangesUI.SetActive(false); 

        foreach (var slot in luggageSlots) 
        {
            if (!slot.IsEmpty)
            {
                slot.ItemOriginalLocation = slot.Location;
            }
        }

        foreach (var slot in shopSlots)
        {
            if (!slot.IsEmpty)
            {
                slot.ItemOriginalLocation = slot.Location;
            }
        }

        CheckStorage();

        luggageAdditions.Clear();
        shopAdditions.Clear();

        moneyChangesUI.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        backButton.SetActive(true);
    }

    public void RejectChanges()
    {
        backButton.SetActive(true);
        moneyChangesUI.SetActive(false);
        moneyChanges = 0;

        foreach (var slot in luggageAdditions.ToList())
        {
            TransferItem(slot);
            luggageAdditions.Remove(slot);
        }

        foreach (var slot in shopAdditions.ToList())
        {
            TransferItem(slot);
            shopAdditions.Remove(slot);
        }

        luggageAdditions.Clear();
        shopAdditions.Clear();
        moneyChanges = 0;

        moneyChangesUI.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }

    public void CheckStorage() 
    {
        foreach (var slot in luggageSlots) 
        {
            slot.Check();
        }

        foreach (var slot in shopSlots) 
        {
            slot.Check();
        }
    }

    public InventorySlot TransferItem(InventorySlot item) 
    {
        InventorySlot updatedSlot = null;

        if (item.Location == "Luggage") 
        {
            foreach (var slot in shopSlots) 
            {
                if (slot.IsEmpty) 
                {
                    slot.GetComponent<Image>().sprite = item.ItemOriginalLocation == "Shop" ? 
                        Resources.Load<Sprite>($"Inventory/{item.IconKey}") : 
                        Resources.Load<Sprite>($"Inventory/Highlights/{item.IconKey}");

                    slot.IconKey = item.IconKey;
                    slot.ItemId = item.ItemId;
                    slot.Location = "Shop";
                    slot.ItemOriginalLocation = item.ItemOriginalLocation;
                    slot.Value = item.Value;
                    slot.Type = item.Type;
                    slot.IsEmpty = false;

                    moneyChanges += item.Value;

                    updatedSlot = slot;
                    item.ResetItem();

                    break;
                }
            }
        }
        else if(item.Location == "Shop") 
        {
            foreach (var slot in luggageSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.GetComponent<Image>().sprite = item.ItemOriginalLocation == "Luggage" ?
                        Resources.Load<Sprite>($"Inventory/{item.IconKey}") :
                        Resources.Load<Sprite>($"Inventory/Highlights/{item.IconKey}");

                    slot.IconKey = item.IconKey;
                    slot.ItemId = item.ItemId;
                    slot.Location = "Luggage";
                    slot.ItemOriginalLocation = item.ItemOriginalLocation;
                    slot.Value = item.Value;
                    slot.Type = item.Type;
                    slot.IsEmpty = false;                    

                    moneyChanges -= item.Value;

                    updatedSlot = slot;
                    item.ResetItem();

                    break;
                }
            }
        }

        pendingMoneyChangeText.GetComponent<Text>().text = moneyChanges.ToString();
        return updatedSlot;
    }
}
