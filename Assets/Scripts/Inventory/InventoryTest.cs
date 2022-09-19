using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public InventoryContainer inventoryContainer;
    public Item testItem;
    public Item testLargeItem;
    public Item testStackableItem;
    public Item testInfiniteStackableItem;

    private void Start()
    {
        inventoryContainer.SetLuggageCount(3);
    }

    private void TestItem(Item item)
    {
        Debug.Log(inventoryContainer.TryAddItem(item));
    }

    public void AddTestItem()
    {
        TestItem(testItem);
    }

    public void AddTestLargeItem()
    {
        TestItem(testLargeItem);
    }

    public void AddTestStackableItem()
    {
        TestItem(testStackableItem);
    }

    public void AddTestInfiniteStackableItem()
    {
        TestItem(testInfiniteStackableItem);
    }

    public void EnableGhostMode()
    {
        inventoryContainer.EnableGhostMode();
    }

    public void ApplyGhostMode()
    {
        inventoryContainer.ApplyGhostMode();
    }

    public void CancelGhostMode()
    {
        inventoryContainer.CancelGhostMode();
    }
}
