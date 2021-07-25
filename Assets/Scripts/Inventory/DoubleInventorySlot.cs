using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleInventorySlot : InventorySlot
{
    public InventorySlot FirstSlot { get; set; }

    public InventorySlot SecondSlot { get; set; }

    public bool Isvertical;

    public override void Check()
    {
        IsEmpty = !ItemId.HasValue;

        if (IsEmpty)
        {
            ResetItem();
        }
        else if (Isvertical)
        {
            if (Location == ItemOriginalLocation && !IsSelected)
            {
                GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}");
            }
            else
            {
                GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/Highlights/{IconKey}");
            }
        }
        else
        {
            if (Location == ItemOriginalLocation && !IsSelected)
            {
                GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/{IconKey}_h");
            }
            else
            {
                GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/Highlights/{IconKey}_h");
            }
        }
    }

    public override void ResetItem()
    {
        FirstSlot?.ResetItem();
        SecondSlot?.ResetItem();

        base.ResetItem();

        Destroy(gameObject);
    }
}
