using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum InventoryGhostChange
{
    /// There was no change.
    None,
    /// The amount changed.
    Amount,
    /// The whole item was added during ghost mode.
    Item
}

public abstract class InventoryManager : MonoBehaviour
{
    public static int GridWidth { get { return 4; } }
    public static int GridHeight { get { return 3; } }

    public UnityEvent<InventorySlot> onSlotClicked = new UnityEvent<InventorySlot>();

    [SerializeField]
    private GameObject ItemSlotPrefab;

    protected int bagCount = 0;
    protected List<InventorySlot> slots = new List<InventorySlot>();
    // Removed inventory slots during ghost mode.
    protected List<InventorySlot> removedSlots = new List<InventorySlot>();
    private bool ghostMode = false;
    private bool blockBroadcast = false;

    public delegate void OnItemAmountChangedEvent(Item item, int amount);
    public event OnItemAmountChangedEvent onItemAmountChanged;

    public int Width { get { return GridWidth; } }
    public int Height { get { return GridHeight * bagCount; } }

    protected virtual void Start()
    {
        if(bagCount == 0)
        {
            SetBagCount(1);
        }
    }

    public virtual void SetBagCount(int newBagCount)
    {
        Debug.Assert(newBagCount >= bagCount);
        bagCount = newBagCount;
    }

    /**
     * Enables ghost mode.
     * Items added in ghost mode are not broadcasted that it was added,
     * so that i.e. the player inventory does not receive the information until ApplyGhostMode was called.
     */
    public void EnableGhostMode()
    {
        ghostMode = true;
    }

    /**
     * Applies ghost mode.
     * Info that items were added is broadcasted.
     */
    public void ApplyGhostMode()
    {
        ghostMode = false;

        // Actually remove the removed slots before the next step.
        foreach (InventorySlot slot in removedSlots)
        {
            if(slot.ChangedAmount != 0)
            {
                // If you add an item from the basket to the luggage and remove it from the luggage again in same ghost mode transaction, this can be 0.
                OnItemAmountChanged(slot.Item, slot.ChangedAmount);
            }
            slots.Remove(slot);
            DestroySlot(slot);
        }
        removedSlots.Clear();

        // Broadcast all changes
        foreach (InventorySlot slot in slots)
        {
            int changedAmount = slot.ChangedAmount;
            InventoryGhostChange change = slot.ApplyGhostMode();
            if (change != InventoryGhostChange.None)
            {
                OnItemAmountChanged(slot.Item, changedAmount);
            }
        }
    }

    /**
     * Cancels ghost mode.
     * Items added in ghost mode are removed.
     */
    public void CancelGhostMode()
    {
        // Remove all slots that were added during ghost mode.
        List<InventorySlot> slotsToRemove = new List<InventorySlot>();
        foreach (InventorySlot slot in slots)
        {
            InventoryGhostChange change = slot.CancelGhostMode();
            switch (change)
            {
                case InventoryGhostChange.Item:
                {
                    // Remove the slot again.
                    slotsToRemove.Add(slot);
                    break;
                }
            }
        }

        foreach (InventorySlot slot in slotsToRemove)
        {
            slots.Remove(slot);
            DestroySlot(slot);
        }

        // Readd all slots that were removed during ghost mode.
        foreach (InventorySlot removedSlot in removedSlots)
        {
            if(removedSlot.Amount > 0)
            {
                TryAttachSlot(removedSlot);
            }
            else
            {
                slots.Remove(removedSlot);
                DestroySlot(removedSlot);
            }
        }
        removedSlots.Clear();

        ghostMode = false;
    }

    private void OnItemAmountChanged(Item item, int changedAmount)
    {
        if(!blockBroadcast && onItemAmountChanged != null && !ghostMode)
        {
            onItemAmountChanged.Invoke(item, changedAmount);
        }
    }

    /**
     * Tries to add an item.
     * Tries to add it to the stack if possible first, then adds a new slot.
     */
    public bool TryAddItem(Item item)
    {
        if(item.IsStackable)
        {
            if (TryAddItemToStack(item) != null)
            {
                OnItemAmountChanged(item, 1);
                return true;
            }
        }

        if(TryAddNewItem(item) != null)
        {
            OnItemAmountChanged(item, 1);
            return true;
        }

        return false;
    }

    private InventorySlot TryAddItemToStack(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            // Only add item to stack on slots that are not currently removed in ghost mode.
            if (slot.Item == item && !removedSlots.Contains(slot))
            {
                if (slot.TryAddToStack(ghostMode))
                {
                    return slot;
                }
            }
        }

        return null;
    }

    private InventorySlot TryAddNewItem(Item item)
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                InventorySlot inventorySlot = GetInventorySlotAt(x, y);
                // Check if the slot is empty (or removed during ghost mode).
                if (inventorySlot == null)
                {
                    bool placed = false;
                    // Check if this is a 1x1 slot or a 2x1.
                    if (item.Volume == 1)
                    {
                        // This is a single 1x1 item.
                        inventorySlot = CreateSlot(item, x, y, 1, 1);
                        placed = true;
                    }
                    else
                    {
                        Debug.Assert(item.Volume == 2);

                        // Try to position the item horizontally at x, y.
                        inventorySlot = TryAddNewItemHorizontallyAt(item, x, y);
                        if (inventorySlot == null)
                        {
                            // Horizontal is not possible, so try to position it vertically.
                            inventorySlot = TryAddNewItemVerticallyAt(item, x, y);
                        }

                        if (inventorySlot != null)
                        {
                            // The item was placed horizontally or vertically.
                            placed = true;
                        }
                    }

                    if (placed)
                    {
                        // The item was placed.
                        slots.Add(inventorySlot);
                        TryAttachSlot(inventorySlot);
                        return inventorySlot;
                    }
                }
            }
        }

        return null;
    }

    private InventorySlot TryAddNewItemHorizontallyAt(Item item, int x, int y)
    {
        InventorySlot inventorySlot = null;

        // Check if the slot right to this one is empty.
        if (x < Width - 1)
        {
            InventorySlot rightSlot = GetInventorySlotAt(x + 1, y);
            if (rightSlot == null)
            {
                // The item can be placed horizontally.
                inventorySlot = CreateSlot(item, x, y, 2, 1);
            }
        }

        return inventorySlot;
    }

    private InventorySlot TryAddNewItemVerticallyAt(Item item, int x, int y)
    {
        InventorySlot inventorySlot = null;
        // Check if the slot on top of this one is empty.
        // If y is right on top of a luggage piece, it can't be placed.
        if (y < Height - 1 && ((y + 1) % GridHeight) != 0)
        {
            InventorySlot upperSlot = GetInventorySlotAt(x, y + 1);
            if (upperSlot == null)
            {
                // The item can be placed vertically.
                inventorySlot = CreateSlot(item, x, y, 1, 2);
            }
        }

        return inventorySlot;
    }

    public bool TryRemoveItemAt(int x, int y)
    {
        InventorySlot slot = GetInventorySlotAt(x, y);
        if (slot == null)
        {
            return false;
        }

        if (slot.TryRemoveFromStack(ghostMode))
        {
            // The amount was decreased.
            OnItemAmountChanged(slot.Item, -1);
            return true;
        }

        // The item is not stackable or its amount reached 0, so remove the slot.
        // Keep a reference in ghost mode if the ghost mode is canceled.
        removedSlots.Add(slot);
        slot.transform.SetParent(null, false);
        slot.gameObject.SetActive(false);
        OnItemAmountChanged(slot.Item, -1);

        return true;
    }

    public void ResetItems(IEnumerable<KeyValuePair<Item, int>> items)
    {
        blockBroadcast = true;

        foreach (InventorySlot slot in slots)
        {
            DestroySlot(slot);
        }
        slots.Clear();

        foreach (KeyValuePair<Item, int> item in items.Where(item => item.Key.Volume > 1))
        {
            // First add all items that are larger than 1 slot.
            for(int i = 0; i < item.Value; i++)
            {
                TryAddItem(item.Key);
            }
        }

        foreach (KeyValuePair<Item, int> item in items.Where(item => item.Key.Volume == 1))
        {
            // Now add all items that occupy only one slot.
            for (int i = 0; i < item.Value; i++)
            {
                TryAddItem(item.Key);
            }
        }

        blockBroadcast = false;
    }

    public void ResetItems(IEnumerable<Item> items)
    {
        blockBroadcast = true;

        foreach(InventorySlot slot in slots)
        {
            DestroySlot(slot);
        }
        slots.Clear();

        foreach(Item item in items.Where(item => item.Volume > 1))
        {
            // First add all items that are larger than 1 slot.
            TryAddItem(item);
        }

        foreach(Item item in items.Where(item => item.Volume == 1))
        {
            // Now add all items that occupy only one slot.
            TryAddItem(item);
        }

        blockBroadcast = false;
    }

    protected void TryAttachSlot(InventorySlot slot)
    {
        if(TryAttachSlotToContainer(slot))
        {
            slot.gameObject.SetActive(true);
        }
        else
        {
            // The newly created slots are added to the world, so hide them until they are added when the user switches luggage.
            slot.gameObject.SetActive(false);
        }
    }

    protected abstract bool TryAttachSlotToContainer(InventorySlot slot);

    protected InventorySlot CreateSlot(Item item, int x, int y, int width, int height)
    {
        GameObject inventorySlotObject = Instantiate(ItemSlotPrefab);
        InventorySlot inventorySlot = inventorySlotObject.GetComponent<InventorySlot>();
        inventorySlot.OnClicked.AddListener((slot) =>
        {
            OnSlotClicked(slot);
        });

        // Sets the width and height, if the item is not 1x1. Assumes only 1x1, 2x1 or 1x2 is possible.
        if (width > 1 || height > 1)
        {
            RectTransform rectTransform = (RectTransform)inventorySlot.transform;
            InventoryContainer container = GetContainer(GetBagIndex(y));
            float adjustment = container.GetSlotSize().x + container.GetSlotMargin();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + adjustment, rectTransform.sizeDelta.y);

            if (width > 1)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + adjustment / 2, rectTransform.anchoredPosition.y);
            }
            else if (height > 1)
            {
                // Rotate
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + adjustment / 2);
                rectTransform.Rotate(Vector3.forward, 90);
            }
        }

        inventorySlot.SetItem(item, x, y, width, height, ghostMode);
        return inventorySlot;
    }

    protected void DestroySlot(InventorySlot slot)
    {
        Destroy(slot.gameObject);
    }

    protected abstract InventoryContainer GetContainer(int bagIndex);

    protected virtual void OnSlotClicked(InventorySlot slot)
    {
        onSlotClicked.Invoke(slot);
    }

    protected InventorySlot GetInventorySlotAt(int x, int y)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsAt(x, y) && !removedSlots.Contains(slot))
            {
                return slot;
            }
        }

        return null;
    }

    protected bool IsInBag(int bagIndex, int y)
    {
        return y >= bagIndex * GridHeight && y < (bagIndex + 1) * GridHeight;
    }

    protected int GetBagIndex(int y)
    {
        return y / GridHeight;
    }
}
