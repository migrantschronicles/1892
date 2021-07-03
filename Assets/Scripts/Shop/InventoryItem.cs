using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public bool empty;
    public string itemName = null;
    public string type; // "Money", "Trade"
    public float value;
    public string location; // Shop or Inventory
    public Sprite image;

    public void Checker()
    {
        Debug.Log(itemName);
        if (itemName == "" || itemName == null || itemName == " ") { 
            empty = true;
            GetComponent<Image>().sprite = null;
        }
        else { 
            empty = false;
            GetComponent<Image>().sprite = image;
        }            
    }

    public void ResetItem() {
        empty = true;
        itemName = null;
        type = null;
        value = 0;
        location = null;
        GetComponent<Image>().sprite = null;
        gameObject.transform.Find("Border").gameObject.SetActive(false);
    }

}
