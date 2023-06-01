using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Provides access to all items that exist in the game.
 * This does not represent the player inventory, this is PlayerInventory.
 */
public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private string folderPrefix = "Items/";

    private Item[] items = null;

    private void Awake()
    {
        items = Resources.LoadAll<Item>(folderPrefix);
    }

    public Item GetItemByTechnicalName(string technicalName)
    {
        foreach(Item item in items)
        {
            if(item.technicalName == technicalName)
            {
                return item;
            }
        }

        return null;
    }

    public Item GetItemById(string id)
    {
        foreach(Item item in items)
        {
            if(item.id == id)
            {
                return item;
            }
        }

        return null;
    }
}
