using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialBlur : MonoBehaviour
{
    [SerializeField]
    private Image blur;
    [SerializeField]
    private UnityEvent pfaffenthal_OnBasketItemClicked;
    [SerializeField]
    private UnityEvent pfaffenthal_OnTransferRight;
    [SerializeField]
    private UnityEvent pfaffenthal_OnAcceptTrade;
    [SerializeField]
    private UnityEvent pfaffenthal_OnItemDragged;

    private GameObject openedPopup;
    private bool pfaffenthal_areItemsBlinking = false;
    private bool pfaffenthal_transferRight = false;
    private bool pfaffenthal_acceptTransfer = false;
    private Dictionary<Button, bool> prevInteractables = new();

    public void SetEnabled(bool value)
    {
        blur.enabled = value;
    }

    public void OpenPopup(GameObject popup)
    {
        SetEnabled(true);
        popup.SetActive(true);
        openedPopup = popup;
    }

    public void ClosePopup(GameObject popup)
    {
        ///@todo
        //if(popup != openedPopup)
        //{
        //    return;
        //}

        popup.SetActive(false);
        SetEnabled(false);
        openedPopup = null;
    }

    public void EnableButton(Button button)
    {
        if(!prevInteractables.ContainsKey(button))
        {
            prevInteractables[button] = button.interactable;
        }

        button.interactable = true;
    }

    public void DisableButton(Button button)
    {
        if(!prevInteractables.ContainsKey(button))
        {
            prevInteractables[button] = button.interactable;
        }

        button.interactable = false;
    }

    public void RestoreButton(Button button)
    {
        if(prevInteractables.ContainsKey(button))
        {
            button.interactable = prevInteractables[button];
            prevInteractables.Remove(button);
        }
    }

    public void MakeItemsBlink(InventoryContainer inventory)
    {
        pfaffenthal_areItemsBlinking = true;
        foreach(Blink blink in inventory.GetComponentsInChildren<Blink>())
        {
            blink.IsRunning = true;
        }
    }

    public void StopItemsBlink(InventoryContainer inventory)
    {
        pfaffenthal_areItemsBlinking = false;
        foreach (Blink blink in inventory.GetComponentsInChildren<Blink>())
        {
            blink.IsRunning = false;
        }
    }

    // Called if a slot in the basket in Pfaffenthal is clicked.
    public void OnBasketItemsClicked()
    {
        if(!pfaffenthal_areItemsBlinking)
        {
            return;
        }

        pfaffenthal_OnBasketItemClicked?.Invoke();
    }

    public void OnTransferRightClicked()
    {
        if(pfaffenthal_transferRight)
        {
            return;
        }

        pfaffenthal_transferRight = true;
        pfaffenthal_OnTransferRight?.Invoke();
    }

    public void OnAcceptTradeClicked()
    { 
        if(pfaffenthal_acceptTransfer || !pfaffenthal_transferRight)
        {
            return;
        }

        pfaffenthal_acceptTransfer = true;
        pfaffenthal_OnAcceptTrade?.Invoke();
    }

    public void OnShopItemDragged()
    {
        if(pfaffenthal_transferRight)
        {
            return;
        }

        pfaffenthal_transferRight = true;
        pfaffenthal_OnItemDragged?.Invoke();
    }
}
