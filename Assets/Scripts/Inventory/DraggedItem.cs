using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggedItem : MonoBehaviour
{
    private Image image;
    private ShopInventorySlot slot;
    private RectTransform rectTransform;
    private ScrollableInventoryManager lastHighlightedManager;

    public ShopInventorySlot Slot
    {
        get { return slot; }
        set
        {
            slot = value;
            image.sprite = slot.InventorySlot.Item.sprite;
            if(slot.InventorySlot.Item.Volume > 1)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * 2, rectTransform.sizeDelta.y);
            }
        }
    }

    public bool IsValidTransfer
    {
        get
        {
            return lastHighlightedManager && lastHighlightedManager != slot.InventorySlot.Manager;
        }
    }

    public ScrollableInventoryManager TargetManager { get { return lastHighlightedManager; } }

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void UpdatePosition(PointerEventData data)
    {
        rectTransform.anchoredPosition = data.position;
        Shop currentShop = LevelInstance.Instance.CurrentShop;
        ScrollableInventoryManager highlightedManager = currentShop ? currentShop.HighlightedInventoryManager : null;
        if(lastHighlightedManager && lastHighlightedManager != highlightedManager)
        {
            lastHighlightedManager.IsHighlighted = false;
            lastHighlightedManager = null;
        }

        if(highlightedManager && highlightedManager != lastHighlightedManager && highlightedManager != slot.InventoryManager)
        {
            highlightedManager.IsHighlighted = true;
            lastHighlightedManager = highlightedManager;
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        UpdatePosition(data);
    }

    public void OnDrag(PointerEventData data)
    {
        UpdatePosition(data);
    }

    public void OnEndDrag(PointerEventData data)
    {
        if(lastHighlightedManager)
        {
            lastHighlightedManager.IsHighlighted = false;
        }
    }
}
