using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public bool isEmpty;
    public string itemName = null;
    public string type; // "Money", "Trade"
    public int value;
    public string location; // Shop or Inventory

    //Resources/Inventory
    public string IconKey { get; set; }

    public void Update()
    {
        if (GetComponent<Image>().sprite == null && !isEmpty)
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
        }
        else if (GetComponent<Image>().sprite != null && isEmpty) 
        {
            GetComponent<Image>().sprite = null;
            IconKey = null;
        }
    }

    public void Checker()
    {
        if (string.IsNullOrWhiteSpace(itemName)) 
        { 
            isEmpty = true;
            GetComponent<Image>().sprite = null;
        }
        else 
        { 
            isEmpty = false;
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
        }            
    }

    public void ResetItem() 
    {
        isEmpty = true;
        itemName = null;
        type = null;
        value = 0;
        location = null;
        GetComponent<Image>().sprite = null;
        Debug.Log("Image should be null");
        gameObject.transform.Find("Border").gameObject.SetActive(false);
    }
}
