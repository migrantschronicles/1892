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
    private Text descriptionText;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private GameObject deleteItemPopupPrefab;

    private InventorySlot selectedSlot = null;

    protected override void Start()
    {
        base.Start();
        bag0.onRemoveItem += OnRemoveItem;
        SetBagCount(NewGameManager.Instance.inventory.GetBagCount());
        ResetItems(NewGameManager.Instance.inventory.Items);
        LevelInstance.Instance.IngameDiary.Diary.onDiaryStatusChanged += (status) =>
        {
            if (status == OpenStatus.Opening)
            {
                ResetItems(NewGameManager.Instance.inventory.Items);
            }
        };
    }

    public override void SetBagCount(int newBagCount)
    {
        base.SetBagCount(newBagCount);
        bag0.SetUnlocked(bagCount >= 1);
    }

    protected override InventoryContainer GetContainer(int bagIndex)
    {
        switch(bagIndex)
        {
            case 0: return bag0;
            default: Debug.Assert(false); return null;
        }
    }

    protected override bool TryAttachSlotToContainer(InventorySlot slot)
    {
        int bagIndex = GetBagIndex(slot.Y);
        InventoryContainer container = GetContainer(bagIndex);
        container.AttachSlot(slot, slot.X, slot.Y % GridHeight);
        return true;
    }

    protected override void OnSlotClicked(InventorySlot slot)
    {
        if(selectedSlot)
        {
            selectedSlot.SetSelected(false);
            selectedSlot = null;
        }

        base.OnSlotClicked(slot);
        string description = LocalizationManager.Instance.GetLocalizedString(slot.Item.Description, slot.Item.Price);
        descriptionText.text = description;
        string itemName = LocalizationManager.Instance.GetLocalizedString(slot.Item.Name);
        nameText.text = itemName;

        selectedSlot = slot;
        selectedSlot.SetSelected(true);
    }

    private void OnRemoveItem(DiaryInventoryContainer container)
    {
        if(!selectedSlot)
        {
            return;
        }

        // Show popup
        GameObject popup = LevelInstance.Instance.ShowPopup(deleteItemPopupPrefab);
        DeleteItemPopup deleteItemPopup = popup.GetComponent<DeleteItemPopup>();
        deleteItemPopup.OnAccepted += (popup) =>
        {
            int bagIndex = GetBagIndex(selectedSlot.Y);
            switch (bagIndex)
            {
                case 0:
                    if (container != bag0) return;
                    break;
            }

            if (TryRemoveItemAt(selectedSlot.X, selectedSlot.Y))
            {
                NewGameManager.Instance.inventory.OnItemAmountChanged(selectedSlot.Item, -1);
                if (GetInventorySlotAt(selectedSlot.X, selectedSlot.Y) == selectedSlot)
                {
                    // The item amount was decreased.
                    selectedSlot.SetSelected(true);
                }
                else
                {
                    descriptionText.text = "";
                    nameText.text = "";
                    selectedSlot = null;
                }
            }

            LevelInstance.Instance.PopPopup();
        };

        deleteItemPopup.OnRejected += (popup) =>
        {
            LevelInstance.Instance.PopPopup();
        };
    }
}
