using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class InventoryContainer : MonoBehaviour
{
    [SerializeField]
    protected GameObject SlotsParent;



    /**
     * @return The local size of one slot
     */
    public Vector2 GetSlotSize()
    {
        RectTransform rectTransform = (RectTransform)SlotsParent.transform.GetChild(0);
        return rectTransform.rect.size;
    }

    /**
     * The margin (in pixels) between two slots.
     */
    public float GetSlotMargin()
    {
        RectTransform rectTransform0 = (RectTransform)SlotsParent.transform.GetChild(0);
        RectTransform rectTransform1 = (RectTransform)SlotsParent.transform.GetChild(1);
        float distance = rectTransform1.anchoredPosition.x - rectTransform0.anchoredPosition.x;
        float margin = distance - rectTransform0.rect.width;
        return margin;
    }

    public void AttachSlot(InventorySlot slot, int x, int y)
    {
        slot.transform.SetParent(SlotsParent.transform.GetChild(y * InventoryManager.GridWidth + x), false);
    }
}
