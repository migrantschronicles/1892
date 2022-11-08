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
                    NewGameManager.Instance.RemoveConditions(item.SetConditions);
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
            items.Add(item, changedAmount);
            if(item.SetConditions != null && item.SetConditions.Length > 0)
            {
                NewGameManager.Instance.AddConditions(item.SetConditions);
            }
        }
    }
}
