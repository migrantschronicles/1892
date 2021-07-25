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

    public virtual void Check()
    {
        IsEmpty = !ItemId.HasValue;

        if (IsEmpty)
        {
            GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/empty");
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

    public virtual void ResetItem() 
    {
        IsEmpty = true;
        ItemId = null;
        Type = null;
        Value = 0;
        Location = null;
        ItemOriginalLocation = null;
        IconKey = "empty";
        GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/empty");
        gameObject.SetActive(true);
    }
}
