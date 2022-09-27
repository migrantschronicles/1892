using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryInventoryManager : InventoryManager
{
    [SerializeField]
    private DiaryInventoryContainer bag0;
    [SerializeField]
    private DiaryInventoryContainer bag1;
    [SerializeField]
    private DiaryInventoryContainer bag2;

    public DiaryInventoryManager()
    {
    }

    protected override void Start()
    {
        base.Start();
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
}
