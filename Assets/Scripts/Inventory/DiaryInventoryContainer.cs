using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryInventoryContainer : InventoryManager
{
    [SerializeField]
    private InventoryContainer bag0;
    [SerializeField]
    private InventoryContainer bag1;
    [SerializeField]
    private InventoryContainer bag2;

    public DiaryInventoryContainer()
    {
    }

    protected override InventoryContainer GetContainer(int bagIndex)
    {
        throw new System.NotImplementedException();
    }

    protected override bool TryAttachSlotToContainer(InventorySlot slot)
    {
        throw new System.NotImplementedException();
    }
}
