using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInventory
{
    [SerializeField]
    private Item[] startItems;

    private Dictionary<Item, int> items = new Dictionary<Item, int>();

    public delegate void OnItemAddedEvent(Item item);
    public OnItemAddedEvent onItemAdded;

    public delegate void OnItemRemovedEvent(Item item);
    public OnItemRemovedEvent onItemRemoved;

    public delegate void OnItemsChanged(IEnumerable<KeyValuePair<Item, int>> items);
    public OnItemsChanged onItemsChanged;

    public IEnumerable<KeyValuePair<Item, int>> Items { get { return items; } }

    public bool IsEmpty { get { return items.Count == 0; } }

    public void Initialize()
    {
        foreach(Item item in startItems)
        {
            if(item != null)
            {
                OnItemAmountChanged(item, 1);
            }
        }
    }

    /**
     * Adds items to the player inventory.
     * Does not broadcast the event.
     */
    public void OnItemAmountChanged(Item item, int changedAmount)
    {
        if(items.TryGetValue(item, out int currentAmount))
        {
            int newAmount = currentAmount + changedAmount;
            Debug.Assert(newAmount >= 0);
            if(newAmount <= 0)
            {
                items.Remove(item);
                if (item.SetConditions != null && item.SetConditions.Length > 0)
                {
                    NewGameManager.Instance.conditions.RemoveConditions(item.SetConditions);
                }

                if(item.category)
                {
                    if (!HasItemCategory(item.category))
                    {
                        if (item.category.SetConditions != null && item.category.SetConditions.Length > 0)
                        {
                            NewGameManager.Instance.conditions.RemoveConditions(item.category.SetConditions);
                        }
                    }
                }
            }
            else
            {
                items[item] = newAmount;
            }
        }
        else
        {
            Debug.Assert(changedAmount > 0);

            if(item.category)
            {
                if (!HasItemCategory(item.category))
                {
                    if (item.category.SetConditions != null && item.category.SetConditions.Length > 0)
                    {
                        NewGameManager.Instance.conditions.AddConditions(item.category.SetConditions, true);
                    }
                }
            }

            items.Add(item, changedAmount);
            if(item.SetConditions != null && item.SetConditions.Length > 0)
            {
                NewGameManager.Instance.conditions.AddConditions(item.SetConditions, true);
            }
        }
    }

    /**
     * @return True if the inventory has the specified item at least once.
     */
    public bool HasItem(Item item)
    {
        if(items.TryGetValue(item, out int amount))
        {
            return amount > 0;
        }

        return false;
    }

    /**
     * @return True if the inventory has an item of a specified type.
     */
    public bool HasItemType(ItemType type)
    {
        foreach(KeyValuePair<Item, int> item in items)
        {
            if(item.Key.ItemType == type && item.Value > 0)
            {
                return true;
            }
        }

        return false;
    }

    /**
     * @return True if the inventory has an item of a specified category.
     */
    public bool HasItemCategory(ItemCategory category)
    {
        foreach(KeyValuePair<Item, int> item in Items)
        {
            if(item.Key.category == category)
            {
                return true;
            }
        }

        return false;
    }

    /**
     * @return The number of items of the specified item.
     */
    public int GetItemCount(Item item)
    {
        if(items.TryGetValue(item, out int amount))
        {
            return amount;
        }

        return 0;
    }
    
    /**
     * @return The total number of items of the specified type.
     */
    public int GetItemTypeCount(ItemType type)
    {
        int amount = 0;
        foreach(KeyValuePair<Item, int> item in items)
        {
            if(item.Key.ItemType == type)
            {
                amount += item.Value;
            }
        }

        return amount;
    }

    /**
     * @return The total number of items of the specified category.
     */
    public int GetItemCategoryCount(ItemCategory category)
    {
        int amount = 0;
        foreach (KeyValuePair<Item, int> item in items)
        {
            if (item.Key.category == category)
            {
                amount += item.Value;
            }
        }

        return amount;
    }

    /**
     * Removes an amount of items.
     * @param item The item to remove
     * @param amount The number of items to remove
     * @return The amount of items removed.
     */
    public int RemoveItem(Item item, int amount = 1)
    {
        Debug.Assert(amount > 0);
        if (items.TryGetValue(item, out int currentAmount))
        {
            amount = Mathf.Min(currentAmount, amount);
            if(amount > 0)
            {
                OnItemAmountChanged(item, -amount);
                return amount;
            }
        }

        return 0;
    }

    /**
     * Removes an amount of items.
     * @param type The item type to remove
     * @param amount The number of items to remove
     * @return The amount of items removed.
     */
    public int RemoveItemType(ItemType type, int amount = 1)
    {
        Debug.Assert(amount > 0);
        List<KeyValuePair<Item, int>> itemsToRemove = new List<KeyValuePair<Item, int>>();
        int removedAmount = 0;
        foreach(KeyValuePair<Item, int> item in items)
        {
            if(item.Key.ItemType == type && item.Value > 0)
            {
                int amountToRemove = Mathf.Min(amount - removedAmount, item.Value);
                itemsToRemove.Add(new KeyValuePair<Item, int>(item.Key, amountToRemove));
                removedAmount += amountToRemove;

                if(amount - removedAmount <= 0)
                {
                    break;
                }
            }
        }

        foreach(KeyValuePair<Item, int> item in itemsToRemove)
        {
            RemoveItem(item.Key, item.Value);
        }

        return removedAmount;
    }

    /**
     * Removes an amount of items.
     * @param category The item category to remove
     * @param amount The number of items to remove
     * @return The amount of items removed.
     */
    public int RemoveItemCategory(ItemCategory category, int amount = 1)
    {
        return RemoveItemType(category.type, amount);
    }

    /**
     * Adds a specified item to the inventory.
     * @param item The item to add.
     * @param amount The amount
     * @return The number of items added
     */
    public int AddItem(Item item, int amount = 1)
    {
        int added = 0;
        for(; added < amount && CanAddItem(item); ++added)
        {
            OnItemAmountChanged(item, 1);
        }

        return added;
    }

    public int GetBagCount()
    {
        ///@todo
        return 1;
    }

    public int GetAvailableSlotCount()
    {
        int bagCount = GetBagCount();
        int bagSlotCount = bagCount * InventoryManager.GridWidth * InventoryManager.GridHeight;
        int filledSlots = 0;
        foreach (var inventoryItem in items)
        {
            if (inventoryItem.Key.IsInfinitlyStackable || inventoryItem.Value == 1)
            {
                // Either the item is infinitly stackable or only 1 amount, so it takes only one slot.
                filledSlots += inventoryItem.Key.Volume;
            }
            else
            {
                // The item is stackable and has a max stack amount
                filledSlots += (inventoryItem.Value / inventoryItem.Key.MaxStackCount +
                    (((inventoryItem.Value % inventoryItem.Key.MaxStackCount) == 0) ? 0 : 1)) * inventoryItem.Key.Volume;
            }
        }

        int availableSlots = bagSlotCount - filledSlots;
        Debug.Assert(availableSlots >= 0);

        return availableSlots;
    }

    public bool CanAddItem(Item item)
    {
        foreach (var inventoryItem in items)
        {
            if (inventoryItem.Key == item && item.IsStackable &&
                (item.IsInfinitlyStackable || ((inventoryItem.Value % inventoryItem.Key.MaxStackCount) != 0)))
            {
                // Can just add it to the stack.
                return true;
            }
        }

        int availableSlotCount = GetAvailableSlotCount();
        return availableSlotCount >= item.Volume;
    }
}
