using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopInventorySlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private InventorySlot inventorySlot;

    public InventorySlot InventorySlot { get { return inventorySlot; } }
    public ScrollableInventoryManager InventoryManager { get { return (ScrollableInventoryManager) InventorySlot.Manager; } }

    private void Awake()
    {
        inventorySlot = GetComponent<InventorySlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //originalPanelLocalPosition = rectTransform.localPosition;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, eventData.position,
        //    eventData.pressEventCamera, out originalLocalPointerPosition);
        LevelInstance.Instance.OnBeginDrag(eventData, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if(RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, eventData.position,
        //    eventData.pressEventCamera, out Vector2 localPointerPosition))
        //{
        //    Vector2 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
        //    offsetToOriginal /= rectTransform.lossyScale;
        //    //rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
        //    Debug.Log(offsetToOriginal);
        //}
        LevelInstance.Instance.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
        LevelInstance.Instance.OnEndDrag(eventData);
    }
}
