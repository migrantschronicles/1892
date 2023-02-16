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
        Debug.Log(items.Length);
    }
}
