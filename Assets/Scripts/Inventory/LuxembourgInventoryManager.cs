using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LuxembourgInventoryManager : InventoryManager
{
    static LuxembourgInventoryManager()
    {
        basketInventoryIds = InventoryData.InventoryById.Where(i => i.Value.Location == InventoryLocation.LuxembougShop).Select(i => i.Key).ToList();
        luggageInventoryIds = StateManager.CurrentState.AvailableItemIds.ToList();
    }

    public LuxembourgInventoryManager()
    {
        basketColums = 4;
        basketRows = 3;

        luggageColums = 4;
        luggageRows = 3;

        considerMoneyChange = true;
    }
}
