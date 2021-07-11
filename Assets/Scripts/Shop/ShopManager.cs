using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<InventorySlot> shopSlots = new List<InventorySlot>(12);
    public List<InventorySlot> luggageSlots = new List<InventorySlot>(12);

    public int moneyChanges = 0;
    public List<InventorySlot> shopAdditions;
    public List<InventorySlot> luggageAdditions;

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
        foreach (var id in StateManager.CurrentState.AvailableItemIds) 
        {
            foreach(var slot in luggageSlots)
            {
                if(slot.IsEmpty)
                {
                    slot.IsEmpty = false;
                    slot.Location = "Luggage";
                    slot.Value = InventoryData.InventoryById[id].Price;
                    slot.IconKey = InventoryData.InventoryById[id].Name;
                    break;
                }
            }
        }

        foreach (var slot in shopSlots)
        {
            slot.IsEmpty = false;
            slot.Location = "Shop";
            slot.Value = InventoryData.InventoryById[10].Price;
            slot.IconKey = InventoryData.InventoryById[10].Name;
            break;
        }
    }

    private void Update()
    {
        if (luggageAdditions.Count == 0 && shopAdditions.Count == 0)
        {
            moneyChanges = 0;
        }
    }


    public void PickItem(InventorySlot item) {
        if (!item.IsEmpty)
        {
            backButton.SetActive(false);
            // Show money changes
            moneyChangesUI.SetActive(true);
            InventorySlot newItem = transferItem(item);

            // Handle temporary placements
            if (newItem.Location == "Shop")
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
        StateManager.CurrentState.AvailableMoney += moneyChanges;
        moneyChanges = 0;
        moneyChangesUI.SetActive(false); 

        // Resetting addition lists
        foreach (InventorySlot item in luggageSlots) 
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); 
            item.GetComponent<Button>().interactable = true;
        }
        foreach (InventorySlot item in shopSlots)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false);
            item.GetComponent<Button>().interactable = true;
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
        Debug.Log("Rejecting 1");
        // Resetting addition lists
        foreach (InventorySlot item in luggageAdditions.ToList())
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            transferItem(item);
            luggageAdditions.Remove(item);
            Debug.Log(item);
        }
        Debug.Log("Rejecting 2");
        foreach (InventorySlot item in shopAdditions.ToList())
        {
            Debug.Log("Rejecting 2.1");
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            Debug.Log("Rejecting 2.2");
            transferItem(item);
            Debug.Log("Rejecting 2.3");
            shopAdditions.Remove(item);
            Debug.Log("Rejecting 2.4");
            Debug.Log(item);
            Debug.Log("Rejecting 2.5");
        }
        Debug.Log("Rejecting 3");

        foreach (InventorySlot item in shopSlots) {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            item.GetComponent<Button>().interactable = true;
        }
        Debug.Log("Rejecting 4");
        foreach (InventorySlot item in luggageSlots)
        {
            item.gameObject.transform.Find("Border").gameObject.SetActive(false); // Remove highlight
            item.GetComponent<Button>().interactable = true;
        }
        Debug.Log("Rejecting 5");
        luggageAdditions.Clear();
        shopAdditions.Clear();
        Debug.Log("Rejecting 6");
        moneyChangesUI.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        moneyChanges = 0;
        Debug.Log("Rejecting Final!");
    }

    public void StorageChecker() {
        foreach (InventorySlot item in luggageSlots) {
            item.Checker();
        }
        foreach (InventorySlot item in shopSlots) {
            item.Checker();
        }
    }

    public InventorySlot transferItem(InventorySlot item) 
    {
        InventorySlot returnedItem = null;
        item.gameObject.transform.Find("Border").gameObject.SetActive(false);
        // Luggage to Shop
        if (item.Location == "Luggage") {
            bool doneFlag = false;
            foreach (InventorySlot item1 in shopSlots) 
            {
                if (item1.IsEmpty && !doneFlag) {
                    item1.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{item.IconKey}");
                    item1.IconKey = item.IconKey;
                    item1.ItemName = item.ItemName;
                    item1.Location = "Shop";
                    item1.Value = item.Value;
                    item1.Type = item.Type;
                    item1.IsEmpty = false;

                    moneyChanges += item.Value;
                    //Highlight
                    item1.gameObject.transform.Find("Border").gameObject.SetActive(true);

                    returnedItem = item1;
                    item.ResetItem();
                    doneFlag = true;
                    item1.GetComponent<Button>().interactable = false;
                }
            }
        }else if(item.Location == "Shop") {
            bool doneFlag = false;
            foreach (InventorySlot item1 in luggageSlots)
            {
                if (item1.IsEmpty && !doneFlag)
                {
                    item1.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{item.IconKey}");
                    item1.IconKey = item.IconKey;
                    item1.ItemName = item.ItemName;
                    item1.Location = "Luggage";
                    item1.Value = item.Value;
                    item1.Type = item.Type;
                    item1.IsEmpty = false;
                    

                    moneyChanges -= item.Value;
                    //Highlight
                    item1.gameObject.transform.Find("Border").gameObject.SetActive(true);
                    //luggageAdditions.Add(item1);
                    returnedItem = item1;
                    item.ResetItem();
                    doneFlag = true;
                    item1.GetComponent<Button>().interactable = false;
                }
            }
        }
        //Update money
        pendingMoneyChangeText.GetComponent<Text>().text = moneyChanges.ToString();
        return returnedItem;
    }
}
