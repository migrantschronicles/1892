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
    private UnityEvent OnDecision;
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
    private UnityEvent pfaffenthal_OnMadameHutainEnded;
    [SerializeField]
    private UnityEvent pfaffenthal_OnLuxembourgDiscoveredDiaryOpening;
    [SerializeField]
    private UnityEvent luxembourg_OnDiaryOpening;
    [SerializeField]
    private UnityEvent luxembourg_OnDiaryOpened;
    [SerializeField]
    private UnityEvent luxembourg_OnInventory;
    [SerializeField]
    private UnityEvent luxembourg_OnSettings;
    [SerializeField]
    private UnityEvent luxembourg_OnHealth;
    [SerializeField]
    private UnityEvent luxembourg_OnDiary;
    [SerializeField]
    private UnityEvent luxembourg_OnNextHealthPage;
    [SerializeField]
    private UnityEvent luxembourg_OnDiaryClosed;
    [SerializeField]
    private UnityEvent luxembourg_OnShopOrDialogClosed;
    [SerializeField]
    private UnityEvent luxembourg_OnSceneButton;

    private GameObject openedPopup;
    private bool decision = false;
    private bool pfaffenthal_areItemsBlinking = false;
    private bool pfaffenthal_transferRight = false;
    private bool pfaffenthal_acceptTransfer = false;
    private bool pfaffenthal_shopClosed = false;
    private bool pfaffenthal_madameHutain = false;
    private bool pfaffenthal_madameHutainEnded = false;
    private bool luxembourg_diaryOpened = false;
    private bool luxembourg_inventory = false;
    private bool luxembourg_settings = false;
    private bool luxembourg_health = false;
    private bool luxembourg_diary = false;
    private bool luxembourg_nextHealthPage = false;
    private bool luxembourg_diaryClosed = false;
    private bool luxembourg_dialog = false;
    private bool luxembourg_shop = false;
    private bool luxembourg_shopOrDialogClosed = false;
    private bool luxembourg_sceneButton = false;
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

    public void ClosePopup()
    {
        openedPopup.SetActive(false);
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
        if (!luxembourg_shopOrDialogClosed && luxembourg_dialog && luxembourg_shop)
        {
            luxembourg_shopOrDialogClosed = true;
            luxembourg_OnShopOrDialogClosed?.Invoke();
        }
        else if(luxembourg_nextHealthPage && !luxembourg_diaryClosed)
        {
            luxembourg_diaryClosed = true;
            luxembourg_OnDiaryClosed?.Invoke();
        }
        else if(pfaffenthal_acceptTransfer && !pfaffenthal_shopClosed)
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

    public void OnMadameHutainEnded()
    {
        if(pfaffenthal_madameHutainEnded)
        {
            return;
        }

        pfaffenthal_madameHutainEnded = true;
        pfaffenthal_OnMadameHutainEnded?.Invoke();
    }

    public void OnLuxembourgDiscoveredDiaryOpening()
    {
        pfaffenthal_OnLuxembourgDiscoveredDiaryOpening?.Invoke();
    }

    public void MakeTransportationMethodsBlink(Map map)
    {
        if(map.TransportationMethods == null)
        {
            return;
        }

        foreach(MethodManager parent in map.TransportationMethods.GetComponentsInChildren<MethodManager>())
        {
            if(!parent.gameObject.activeSelf)
            {
                continue;
            }

            Blink blink = parent.GetComponent<Blink>();
            if(blink == null)
            {
                blink = parent.gameObject.AddComponent<Blink>();
            }

            blink.IsRunning = true;

            blink = parent.transform.GetChild(0).GetComponent<Blink>();
            if(blink == null)
            {
                blink = parent.transform.GetChild(0).gameObject.AddComponent<Blink>();
            }

            blink.IsRunning = true;
        }
    }

    public void CompleteFeatureClock()
    {
        TutorialManager.Instance.CompleteFeature(TutorialFeature.ClockUnlocked);
    }

    public void CompleteFeatureDiary()
    {
        TutorialManager.Instance.CompleteFeature(TutorialFeature.DiaryUnlocked);
    }

    public void OnDiaryOpening()
    {
        if(luxembourg_diaryOpened)
        {
            return;
        }

        luxembourg_diaryOpened = true;
        LevelInstance.Instance.UI.IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;
        luxembourg_OnDiaryOpening?.Invoke();
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Opened)
        {
            LevelInstance.Instance.UI.IngameDiary.Diary.onDiaryStatusChanged -= OnDiaryStatusChanged;
            luxembourg_OnDiaryOpened?.Invoke();
        }
    }

    public void OnInventory()
    {
        if(luxembourg_inventory)
        {
            return;
        }

        luxembourg_inventory = true;
        luxembourg_OnInventory?.Invoke();
    }

    public void OnHealth()
    {
        if(!luxembourg_health)
        {
            luxembourg_health = true;
            luxembourg_OnHealth?.Invoke();
        }
    }

    public void OnDiary()
    {
        if(!luxembourg_diary)
        {
            luxembourg_diary = true;
            luxembourg_OnDiary?.Invoke();
        }
    }

    public void OnSettings()
    {
        if(!luxembourg_settings)
        {
            luxembourg_settings = true;
            luxembourg_OnSettings?.Invoke();
        }
    }

    public void OnNextHealthPage()
    {
        if(!luxembourg_nextHealthPage)
        {
            luxembourg_nextHealthPage = true;
            luxembourg_OnNextHealthPage?.Invoke();
        }
    }

    public void OnLuxembourgDialog()
    {
        luxembourg_dialog = true;
    }

    public void OnLuxembourgShop()
    {
        luxembourg_shop = true;
    }

    public void OnLuxembourgSceneButton()
    {
        if(!luxembourg_sceneButton)
        {
            luxembourg_sceneButton = true;
            luxembourg_OnSceneButton?.Invoke();
        }
    }
}
