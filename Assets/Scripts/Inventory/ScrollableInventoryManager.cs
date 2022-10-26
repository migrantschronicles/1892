using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollableInventoryManager : InventoryManager
{
    [SerializeField]
    private InventoryContainer container;
    [SerializeField]
    private Button bagUpButton;
    [SerializeField]
    private Button bagDownButton;
    [SerializeField]
    private Color defaultBagButtonColor = Color.white;
    [SerializeField]
    private Color enabledBagButtonColor = Color.white;
    [SerializeField]
    private Image bagImage;
    [SerializeField]
    private Sprite[] bagImages;

    private int currentBagIndex = 0;

    private bool CanScrollUp
    {
        get
        {
            return currentBagIndex < bagCount - 1;
        }
    }

    private bool CanScrollDown
    {
        get
        {
            return currentBagIndex > 0;
        }
    }

    protected override void Start()
    {
        base.Start();
        bagUpButton.onClick.AddListener(OnBagUpClicked);
        bagDownButton.onClick.AddListener(OnBagDownClicked);
    }

    public override void SetBagCount(int newBagCount)
    {
        base.SetBagCount(newBagCount);
        bagUpButton.gameObject.SetActive(bagCount > 1);
        bagDownButton.gameObject.SetActive(bagCount > 1);
        SetCurrentBagIndex(currentBagIndex);
    }

    private void SetCurrentBagIndex(int value)
    {
        // Detach and hide all current inventory slots
        foreach (InventorySlot slot in slots)
        {
            if (IsInBag(currentBagIndex, slot.Y))
            {
                slot.transform.SetParent(null, false);
                slot.gameObject.SetActive(false);
            }
        }

        currentBagIndex = value;
        bagUpButton.targetGraphic.color = CanScrollUp ? enabledBagButtonColor : defaultBagButtonColor;
        bagDownButton.targetGraphic.color = CanScrollDown ? enabledBagButtonColor : defaultBagButtonColor;
        bagUpButton.enabled = CanScrollUp;
        bagDownButton.enabled = CanScrollDown;

        // Set the background image if necessary.
        if(bagImages != null && bagImages.Length > 0)
        {
            bagImage.sprite = bagImages[currentBagIndex % bagImages.Length];
        }

        // Show all inventory slots of the new Bag
        foreach (InventorySlot slot in slots)
        {
            if(!removedSlots.Contains(slot))
            {
                TryAttachSlot(slot);
            }
        }
    }

    private void OnBagUpClicked()
    {
        if (currentBagIndex < bagCount - 1)
        {
            SetCurrentBagIndex(currentBagIndex + 1);
        }
    }

    private void OnBagDownClicked()
    {
        if (currentBagIndex > 0)
        {
            SetCurrentBagIndex(currentBagIndex - 1);
        }
    }

    protected override bool TryAttachSlotToContainer(InventorySlot slot)
    {
        if(IsInBag(currentBagIndex, slot.Y))
        {
            int adjustedY = slot.Y - currentBagIndex * GridHeight;
            container.AttachSlot(slot, slot.X, adjustedY);
            return true;
        }

        return false;
    }

    protected override InventoryContainer GetContainer(int bagIndex)
    {
        return container;
    }
}
