using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public InventoryContainer inventoryContainer;
    public Item testItem;
    public Item testLargeItem;

    public void AddTestItem()
    {
        Debug.Log(inventoryContainer.TryAddItem(testItem));
    }

    public void AddTestLargeItem()
    {
        Debug.Log(inventoryContainer.TryAddItem(testLargeItem));
    }
}
