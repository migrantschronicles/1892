using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{

    public List<InventoryItem> shopItems = new List<InventoryItem>(12);
    public List<InventoryItem> luggageItems = new List<InventoryItem>(12);

    public float totalMoney = 50;


    public float moneyChanges = 0;
    public List<InventoryItem> shopAdditions;
    public List<InventoryItem> luggageAdditions;

    [Space(20)]
    public GameObject moneyChangesUI;
    public Text pendingMoneyChangeText;
    public Text moneyText;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject backButton;


    // Start is called before the first frame update
    void Start()
    {
        foreach (InventoryItem item in luggageItems) {
            item.location = "Luggage";
        }
        foreach (InventoryItem item in shopItems)
        {
            item.location = "Shop";
        }
    }

    private void Update()
    {
        moneyText.GetComponent<Text>().text = totalMoney.ToString();

        if (luggageAdditions.Count == 0 && shopAdditions.Count == 0)
            moneyChanges = 0;

    }


    public void PickItem(InventoryItem item) {
        if (!item.empty)
        {
            backButton.SetActive(false);
            // Show money changes
            moneyChangesUI.SetActive(true);
            InventoryItem newItem = transferItem(item);

            // Handle temporary placements
            if (newItem.location == "Shop")
            {
                luggageAdditions.Add(newItem);
            }
            else
            {
                shopAdditions.Add(newItem);
            }
        }
    }

    public void AcceptChanges() {

        // Updating the money
        totalMoney += moneyChanges;
        moneyChanges = 0;
        moneyChangesUI.SetActive(false); //Turning off money UI

        // Resetting addition lists
        foreach (InventoryItem item in luggageItems) 
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
        }
        foreach (InventoryItem item in shopItems)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
        }

        luggageAdditions.Clear();
        shopAdditions.Clear();

        moneyChangesUI.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        backButton.SetActive(true);
    }

    public void RejectChanges()
    {
        backButton.SetActive(true);
        moneyChangesUI.SetActive(false); //Turning off money UI
        moneyChanges = 0;

        // Resetting addition lists
        foreach (InventoryItem item in luggageAdditions)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            transferItem(item);
            luggageAdditions.Remove(item);
            Debug.Log(item);
        }
        foreach (InventoryItem item in shopAdditions)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            transferItem(item);
            shopAdditions.Remove(item);
            Debug.Log(item);
        }

        foreach (InventoryItem item in shopItems) {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
        }
        foreach (InventoryItem item in luggageItems)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
        }

        luggageAdditions.Clear();
        shopAdditions.Clear();

        moneyChangesUI.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        moneyChanges = 0;
    }

    public void StorageChecker() {
        foreach (InventoryItem item in luggageItems) {
            item.Checker();
        }
        foreach (InventoryItem item in shopItems) {
            item.Checker();
        }
    }

    public InventoryItem transferItem(InventoryItem item) 
    {
        InventoryItem returnedItem = null;
        item.gameObject.transform.Find("Border").gameObject.SetActive(false);
        // Luggage to Shop
        if (item.location == "Luggage") {
            bool doneFlag = false;
            foreach (InventoryItem item1 in shopItems) 
            {
                if (item1.empty && !doneFlag) {
                    item1.itemName = item.itemName;
                    item1.location = "Shop";
                    item1.value = item.value;
                    item1.type = item.type;
                    item1.empty = false;
                    item1.GetComponent<Image>().sprite = item.image;

                    moneyChanges += item.value;
                    //Highlight
                    item1.gameObject.transform.Find("Border").gameObject.SetActive(true);

                    returnedItem = item1;
                    item.ResetItem();
                    doneFlag = true;
                }
            }
        }else if(item.location == "Shop") {
            bool doneFlag = false;
            foreach (InventoryItem item1 in luggageItems)
            {
                if (item1.empty && !doneFlag)
                {
                    item1.itemName = item.itemName;
                    item1.location = "Luggage";
                    item1.value = item.value;
                    item1.type = item.type;
                    item1.empty = false;
                    item1.GetComponent<Image>().sprite = item.image;

                    moneyChanges -= item.value;
                    //Highlight
                    item1.gameObject.transform.Find("Border").gameObject.SetActive(true);
                    //luggageAdditions.Add(item1);
                    returnedItem = item1;
                    item.ResetItem();
                    doneFlag = true;
                }
            }
        }
        //Update money
        pendingMoneyChangeText.GetComponent<Text>().text = moneyChanges.ToString();
        return returnedItem;
    }
}
