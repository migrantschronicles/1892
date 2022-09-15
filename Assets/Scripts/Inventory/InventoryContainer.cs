using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
    public static readonly int Width = 4;
    public static readonly int Height = 3;

    [SerializeField]
    private GameObject SlotsParent;
    [SerializeField]
    private GameObject ItemSlotPrefab;

    private List<InventorySlot> inventorySlots = new List<InventorySlot> ();

    public bool TryAddItem(Item item)
    {
        return TryAddNewItem(item) != null;
    }

    private InventorySlot TryAddNewItem(Item item)
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                InventorySlot inventorySlot = GetInventorySlotAt(x, y);
                // Check if the slot is empty.
                if (inventorySlot == null)
                {
                    bool placed = false;
                    // Check if this is a 1x1 slot or a 2x1.
                    if (item.Volume == 1)
                    {
                        // This is a single 1x1 item.
                        inventorySlot = CreateInventorySlot(item, x, y, 1, 1, 1);
                        inventorySlot.transform.SetParent(SlotsParent.transform.GetChild(y * Width + x), false);
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
                        inventorySlots.Add(inventorySlot);
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
                inventorySlot = CreateInventorySlot(item, x, y, 2, 1, 1);
                inventorySlot.transform.SetParent(SlotsParent.transform.GetChild(y * Width + x), false);
            }
        }

        return inventorySlot;
    }

    private InventorySlot TryAddNewItemVerticallyAt(Item item, int x, int y)
    {
        InventorySlot inventorySlot = null;
        // Check if the slot on top of this one is empty.
        if (y < Height - 1)
        {
            InventorySlot upperSlot = GetInventorySlotAt(x, y + 1);
            if (upperSlot == null)
            {
                // The item can be placed vertically.
                inventorySlot = CreateInventorySlot(item, x, y, 1, 2, 1);
                inventorySlot.transform.SetParent(SlotsParent.transform.GetChild(y * Width + x), false);
            }
        }

        return inventorySlot;
    }

    /**
     * @return The local size of one slot
     */
    private Vector2 GetSlotSize()
    {
        RectTransform rectTransform = (RectTransform)SlotsParent.transform.GetChild(0);
        return rectTransform.rect.size;
    }

    /**
     * The margin (in pixels) between two slots.
     */
    private float GetSlotMargin()
    {
        RectTransform rectTransform0 = (RectTransform)SlotsParent.transform.GetChild(0);
        RectTransform rectTransform1 = (RectTransform)SlotsParent.transform.GetChild(1);
        float scaledWidth = rectTransform1.position.x - rectTransform0.position.x;
        float unscaledWidth = scaledWidth / rectTransform0.lossyScale.x;
        return unscaledWidth - rectTransform0.rect.width;
    }

    private InventorySlot CreateInventorySlot(Item item, int x, int y, int width, int height, int amount)
    {
        GameObject inventorySlotObject = Instantiate(ItemSlotPrefab);
        InventorySlot inventorySlot = inventorySlotObject.GetComponent<InventorySlot>();

        // Sets the width and height, if the item is not 1x1. Assumes only 1x1, 2x1 or 1x2 is possible.
        if(width > 1 || height > 1)
        {
            RectTransform rectTransform = (RectTransform)inventorySlot.transform;
            float adjustment = GetSlotSize().x + GetSlotMargin();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + adjustment, rectTransform.sizeDelta.y);

            if(width > 1)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + adjustment / 2, rectTransform.anchoredPosition.y);
            }
            else if(height > 1)
            {
                // Rotate
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + adjustment / 2);
                rectTransform.Rotate(Vector3.forward, 90);
            }
        }

        inventorySlot.SetItem(item, x, y, width, height, amount);
        return inventorySlot;
    }

    private InventorySlot GetInventorySlotAt(int x, int y)
    {
        foreach(InventorySlot slot in inventorySlots)
        {
            if(slot.IsAt(x, y))
            {
                return slot;
            }
        }

        return null;
    }
}
