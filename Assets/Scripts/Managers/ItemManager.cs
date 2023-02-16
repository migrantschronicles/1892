using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
