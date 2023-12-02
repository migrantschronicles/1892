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
    [SerializeField]
    private UnityEvent pfaffenthal_OnShopClosed;
    [SerializeField]
    private UnityEvent pfaffenthal_OnShopOpened;
    [SerializeField]
    private UnityEvent pfaffenthal_OnMadameHutain;
    [SerializeField]
    private UnityEvent OnDecision;

    private GameObject openedPopup;
    private bool pfaffenthal_areItemsBlinking = false;
    private bool pfaffenthal_transferRight = false;
    private bool pfaffenthal_acceptTransfer = false;
    private bool pfaffenthal_shopClosed = false;
    private bool pfaffenthal_madameHutain = false;
    private bool decision = false;
    private Dictionary<Button, bool> prevInteractables = new();

    private void Start()
    {
        DialogSystem.Instance.onDialogDecision += OnDialogDecision;
    }

    private void OnDestroy()
    {
        if(DialogSystem.Instance)
        {
            DialogSystem.Instance.onDialogDecision -= OnDialogDecision;
        }
    }

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

    public void EnableChilds(GameObject parent)
    {
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            Button button = parent.transform.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                EnableButton(button);
            }
        }
    }

    public void RestoreChilds(GameObject parent)
    {
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            Button button = parent.transform.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                RestoreButton(button);
            }
        }
    }

    public void DisableChilds(GameObject parent)
    {
        for(int i = 0; i < parent.transform.childCount; ++i)
        {
            Button button = parent.transform.GetChild(i).GetComponent<Button>();
            if(button != null)
            {
                DisableButton(button);
            }
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

    public void OnExitClicked()
    {
        if(pfaffenthal_acceptTransfer && !pfaffenthal_shopClosed)
        {
            pfaffenthal_shopClosed = true;
            pfaffenthal_OnShopClosed?.Invoke();
        }
    }

    public void OnShopOpened()
    {
        if(pfaffenthal_acceptTransfer)
        {
            return;
        }

        pfaffenthal_OnShopOpened?.Invoke();
    }

    public void OnMadameHutain()
    {
        if(pfaffenthal_madameHutain)
        {
            return;
        }

        pfaffenthal_madameHutain = true;
        pfaffenthal_OnMadameHutain?.Invoke();
    }

    private void OnDialogDecision()
    {
        if (decision)
        {
            return;
        }

        DialogSystem.Instance.onDialogDecision -= OnDialogDecision;
        decision = true;
        OnDecision?.Invoke();
    }

    public void SetClockVisible(bool visible)
    {

    }

    public void SetDiaryVisible(bool visible)
    {

    }
}
