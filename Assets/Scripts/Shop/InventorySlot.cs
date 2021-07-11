using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public bool IsEmpty { get; set; } = true;
    public string ItemName { get; set; }
    public string Type { get; set; }
    public int Value { get; set; }
    public string Location { get; set; }

    //Resources/Inventory
    public string IconKey { get; set; }

    public void Update()
    {
        if (GetComponent<Image>().sprite == null && !IsEmpty)
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
        }
        else if (GetComponent<Image>().sprite != null && IsEmpty) 
        {
            GetComponent<Image>().sprite = null;
            IconKey = null;
        }
    }

    public void Checker()
    {
        if (string.IsNullOrWhiteSpace(ItemName)) 
        { 
            IsEmpty = true;
            GetComponent<Image>().sprite = null;
        }
        else 
        { 
            IsEmpty = false;
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
        }            
    }

    public void ResetItem() 
    {
        IsEmpty = true;
        ItemName = null;
        Type = null;
        Value = 0;
        Location = null;
        GetComponent<Image>().sprite = null;
        Debug.Log("Image should be null");
        gameObject.transform.Find("Border").gameObject.SetActive(false);
    }
}
