using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool BeingDragged;
    public Vector3 Offset;
    public Transform RelatingObject;

    void Update()
    {
        // Moving objects relating to mouse position and calculating in the offset to the object center
        if (BeingDragged)
        {
            RelatingObject.position = Input.mousePosition + Offset;
        }
    }


    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        Offset = RelatingObject.position - Input.mousePosition;
        BeingDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        BeingDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        BeingDragged = false;
    }
}
