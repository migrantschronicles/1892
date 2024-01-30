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
    private UnityEvent OnEndOfDay;
    [SerializeField]
    private UnityEvent OnEndOfDay_Hostel;
    [SerializeField]
    private UnityEvent OnEndOfDay_Outside;
    [SerializeField]
    private UnityEvent OnEndOfDay_Ship;
    [SerializeField]
    private UnityEvent OnClockButton;
    [SerializeField]
    private UnityEvent pfaffenthal_OnStarted;
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
    private UnityEvent pfaffenthal_OnIntroductoryDialog;
    [SerializeField]
    private UnityEvent pfaffenthal_OnIntroductoryDialogEnded;
    [SerializeField]
    private UnityEvent pfaffenthal_OnIntroductoryDialogExited;
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
    [SerializeField]
    private UnityEvent luxembourg_OnOpenAgain;

    private GameObject openedPopup;
    private bool decision = false;
    private bool pfaffenthal_areItemsBlinking = false;
    private bool pfaffenthal_transferRight = false;
    private bool pfaffenthal_acceptTransfer = false;
    private bool pfaffenthal_shopClosed = false;
    private bool pfaffenthal_madameHutain = false;
    private bool pfaffenthal_madameHutainEnded = false;
    private bool pfaffenthal_introductoryDialog = false;
    private bool pfaffenthal_introductoryDialogEnded = false;
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
    private bool luxembourg_openAgain = false;
    private bool luxembourg_ticketSeller = false;
    private bool luxembourg_ticketSellerClosed = false;
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
        else if(LevelInstance.Instance.LocationName == "Luxembourg" && !luxembourg_openAgain && pfaffenthal_introductoryDialog)
        {
            pfaffenthal_introductoryDialogEnded = true;
            luxembourg_openAgain = true;
            luxembourg_OnOpenAgain?.Invoke();
        }
        else if(pfaffenthal_acceptTransfer && !pfaffenthal_shopClosed)
        {
            pfaffenthal_shopClosed = true;
            pfaffenthal_OnShopClosed?.Invoke();
        }
        else if(pfaffenthal_introductoryDialog && !pfaffenthal_introductoryDialogEnded)
        {
            pfaffenthal_OnIntroductoryDialogExited?.Invoke();
            pfaffenthal_introductoryDialogEnded = true;
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

    private void OnDialogDecision(string technicalName)
    {
        if (TutorialManager.Instance.HasCompleted(TutorialFeature.DialogDecision))
        {
            return;
        }

        DialogSystem.Instance.onDialogDecision -= OnDialogDecision;
        TutorialManager.Instance.CompleteFeature(TutorialFeature.DialogDecision);
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
        if(!luxembourg_diary && pfaffenthal_introductoryDialogEnded)
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
            if(NewGameManager.Instance.isHistoryMode)
            {
                EndOfDay();
            }
            else
            {
                // In history mode you can't talk to ticket sellers, so don't show the popup.
                luxembourg_OnSceneButton?.Invoke();
            }
        }
    }

    public void EndOfDay()
    {
        if(!TutorialManager.Instance.HasCompleted(TutorialFeature.EndOfDay))
        {
            TutorialManager.Instance.CompleteFeature(TutorialFeature.EndOfDay);
            OnEndOfDay?.Invoke();
        }
    }

    public void EndOfDay_Hostel()
    {
        if(!TutorialManager.Instance.HasCompleted(TutorialFeature.EndOfDay_Hostel))
        {
            TutorialManager.Instance.CompleteFeature(TutorialFeature.EndOfDay_Hostel);
            OnEndOfDay_Hostel?.Invoke();
        }
    }

    public void EndOfDay_Outside()
    {
        if(!TutorialManager.Instance.HasCompleted(TutorialFeature.EndOfDay_Outside))
        {
            TutorialManager.Instance.CompleteFeature(TutorialFeature.EndOfDay_Outside);
            OnEndOfDay_Outside?.Invoke();
        }
    }

    public void EndOfDay_Ship()
    {
        if(!TutorialManager.Instance.HasCompleted(TutorialFeature.EndOfDay_Ship))
        {
            TutorialManager.Instance.CompleteFeature(TutorialFeature.EndOfDay_Ship);
            OnEndOfDay_Ship?.Invoke();
        }
    }

    public void ClockButton()
    {
        if(TutorialManager.Instance.HasCompleted(TutorialFeature.EndOfDay) && !TutorialManager.Instance.HasCompleted(TutorialFeature.ClockButton))
        {
            TutorialManager.Instance.CompleteFeature(TutorialFeature.ClockButton);
            OnClockButton?.Invoke();
        }
        else if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
        {
            EndOfDay_Ship();
        }
    }

    public void Pfaffenthal_OnStarted()
    {
        pfaffenthal_OnStarted?.Invoke();
    }

    public void OnIntroductoryDialog()
    {
        if(!pfaffenthal_introductoryDialog)
        {
            pfaffenthal_OnIntroductoryDialog?.Invoke();
            pfaffenthal_introductoryDialog = true;
        }
    }

    public void OnIntroductoryDialogEnded()
    {
        if(!pfaffenthal_introductoryDialogEnded)
        {
            pfaffenthal_OnIntroductoryDialogEnded?.Invoke();
            pfaffenthal_introductoryDialogEnded = true;
        }
    }

    public void SetDialogCloseButtonBlinking(bool value)
    {
        if(DialogSystem.Instance.CurrentChat != null && DialogSystem.Instance.CurrentChat.CloseButton != null)
        {
            Blink blink = DialogSystem.Instance.CurrentChat.CloseButton.GetComponent<Blink>();
            if(blink == null)
            {
                if(!value)
                {
                    return;
                }

                blink = DialogSystem.Instance.CurrentChat.CloseButton.gameObject.AddComponent<Blink>();
            }

            blink.IsRunning = value;
        }
    }

    public void OnDialogClosed()
    {
        if(!luxembourg_ticketSellerClosed && luxembourg_ticketSeller)
        {
            luxembourg_ticketSellerClosed = true;
            EndOfDay();
        }
    }

    public void OnLuxembourgTicketSeller()
    {
        if(!luxembourg_ticketSeller)
        {
            luxembourg_ticketSeller = true;
        }
    }
}
