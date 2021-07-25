using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PfaffenthalInventoryManager : InventoryManager
{
    static PfaffenthalInventoryManager()
    {
        basketInventoryIds = InventoryData.InventoryById.Where(i => i.Value.Location == InventoryLocation.AtHome).Select(i => i.Key).ToList();
        luggageInventoryIds = StateManager.CurrentState.AvailableItemIds.ToList();
    }

    public PfaffenthalInventoryManager()
    {
        basketColums = 4;
        basketRows = 4;

        luggageColums = 4;
        luggageRows = 3;
    }
}
