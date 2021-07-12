using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public bool IsEmpty { get; set; } = true;
    public int? ItemId { get; set; }
    public string Type { get; set; }
    public int Value { get; set; }
    public string Location { get; set; }
    public string ItemOriginalLocation { get; set; }

    //Resources/Inventory
    //Resources/Inventory/Highlights
    public string IconKey { get; set; }

    public void Update()
    {
        if (GetComponent<Image>().sprite == null && !IsEmpty)
        {
            Sprite sprite = null;

            if (Location == ItemOriginalLocation)
            {
                sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
            }
            else
            {
                sprite = Resources.Load<Sprite>($"Inventory/Highlights/{IconKey}");
            }

            GetComponent<Image>().sprite = sprite;
        }
        else if (GetComponent<Image>().sprite != null && IsEmpty) 
        {
            GetComponent<Image>().sprite = null;
            IconKey = null;
        }
    }

    public void Check()
    {
        IsEmpty = !ItemId.HasValue;

        if (IsEmpty)
        {
            GetComponent<Image>().sprite = null;
        }
        else if (Location == ItemOriginalLocation)
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/Highlights/{IconKey}");
        }         
    }

    public void ResetItem() 
    {
        IsEmpty = true;
        ItemId = null;
        Type = null;
        Value = 0;
        Location = null;
        ItemOriginalLocation = null;
        GetComponent<Image>().sprite = null;
    }
}
