using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class DiaryInventoryManager : InventoryManager
{
    [SerializeField]
    private DiaryInventoryContainer bag0;
    [SerializeField]
    private DiaryInventoryContainer bag1;
    [SerializeField]
    private DiaryInventoryContainer bag2;
    [SerializeField]
    private Text descriptionText;

    private InventorySlot selectedSlot = null;

    protected override void Start()
    {
        base.Start();
        bag0.onRemoveItem += OnRemoveItem;
        bag1.onRemoveItem += OnRemoveItem;
        bag2.onRemoveItem += OnRemoveItem;
        SetBagCount(3);
        ResetItems(NewGameManager.Instance.inventory.Items);
    }

    public override void SetBagCount(int newBagCount)
    {
        base.SetBagCount(newBagCount);
        bag0.SetUnlocked(bagCount >= 1);
        bag1.SetUnlocked(bagCount >= 2);
        bag2.SetUnlocked(bagCount >= 3);
    }

    protected override InventoryContainer GetContainer(int bagIndex)
    {
        switch(bagIndex)
        {
            case 0: return bag0;
            case 1: return bag1;
            case 2: return bag2;
            default: Debug.Assert(false); return null;
        }
    }

    protected override bool TryAttachSlotToContainer(InventorySlot slot)
    {
        int bagIndex = GetBagIndex(slot.Y);
        InventoryContainer container = GetContainer(bagIndex);
        container.AttachSlot(slot, slot.X, slot.Y);
        return true;
    }

    protected override void OnSlotClicked(InventorySlot slot)
    {
        if(selectedSlot)
        {
            selectedSlot.CancelGhostMode();
            selectedSlot = null;
        }

        base.OnSlotClicked(slot);
        string description = LocalizationManager.Instance.GetLocalizedString(slot.Item.Description, slot.Item.Price);
        descriptionText.text = description;

        selectedSlot = slot;
        selectedSlot.EnableGhostMode();
    }

    private void OnRemoveItem(DiaryInventoryContainer container)
    {
        if(!selectedSlot)
        {
            return;
        }

        int bagIndex = GetBagIndex(selectedSlot.Y);
        switch(bagIndex)
        {
            case 0:
                if (container != bag0) return;
                break;

            case 1:
                if (container != bag1) return;
                break;

            case 2:
                if (container != bag2) return;
                break;
        }

        if(TryRemoveItemAt(selectedSlot.X, selectedSlot.Y))
        {
            NewGameManager.Instance.inventory.OnItemAmountChanged(selectedSlot.Item, -1);
            if(GetInventorySlotAt(selectedSlot.X, selectedSlot.Y) == selectedSlot)
            {
                // The item amount was decreased.
                selectedSlot.EnableGhostMode();
            }
            else
            {
                descriptionText.text = "";
                selectedSlot = null;
            }
        }
    }
}
