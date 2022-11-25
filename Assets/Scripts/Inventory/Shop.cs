using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField]
    private ScrollableInventoryManager Basket;
    [SerializeField]
    private ScrollableInventoryManager Luggage;
    [SerializeField]
    private Button tradeButton;
    [SerializeField]
    private Text descriptionText;
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Button transferLeftButton;
    [SerializeField]
    private Button transferRightButton;
    [SerializeField]
    private Item[] ShopItems;
    [SerializeField]
    private Item shopRequiresItem;
    [SerializeField, Tooltip("True if this is a shop where transfering items does not cost anything (shops during dialogs)")]
    private bool freeShop;

    public AudioClip openClip;
    public AudioClip closeClip;
    [Tooltip("The transfer is accepted, no money is spent nor received")]
    public AudioClip acceptTransferClip;
    [Tooltip("The user wanted to accept the transfer, but does not have enough money")]
    public AudioClip notEnoughMoneyClip;
    [Tooltip("The user accepted the transfer and sold items")]
    public AudioClip receivedMoneyClip;
    [Tooltip("The user accepted the transfer and spent money")]
    public AudioClip spentMoneyClip;

    private bool transferInProgress = false;
    /// The changes during a transfer.
    /// Positive values mean from basket to luggage, negative values mean from luggage to basket.
    private Dictionary<Item, int> transferChanges = new Dictionary<Item, int>();
    private InventorySlot selectedItem;
    private bool selectedItemIsInLuggage = false;
    private Dictionary<Item, int> basketItems = new Dictionary<Item, int>();
    
    public ScrollableInventoryManager HighlightedInventoryManager { get; private set; }

    private bool MeetsRequiredItems
    {
        get
        {
            if(shopRequiresItem)
            {
                if(!Basket.HasItem(shopRequiresItem))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private bool CanClose
    {
        get
        {
            return MeetsRequiredItems;
        }
    }

    private bool CanAccept
    {
        get
        {
            if(freeShop)
            {
                return true;
            }

            int price = CalculatePrice();
            return (price < 0 || NewGameManager.Instance.money >= price);
        }
    }

    private void Awake()
    {
        Basket.onSlotClicked.AddListener(OnBasketItemClicked);
        Luggage.onSlotClicked.AddListener(OnLuggageItemClicked);
        tradeButton.onClick.AddListener(AcceptTransfer);
        transferLeftButton.onClick.AddListener(OnTransferLeft);
        transferRightButton.onClick.AddListener(OnTransferRight);

        Basket.SetBagCount(1);
        Luggage.SetBagCount(3);

        Basket.onItemAmountChanged += OnBasketItemAmountChanged;
        Luggage.onItemAmountChanged += OnLuggageItemAmountChanged;
        Basket.onPointerEnter += OnPointerEnter;
        Basket.onPointerExit += OnPointerExit;
        Luggage.onPointerEnter += OnPointerEnter;
        Luggage.onPointerExit += OnPointerExit;

        foreach(Item item in ShopItems)
        {
            OnBasketItemAmountChanged(item, 1);
        }
    }

    private void Start()
    {
        UpdateDynamics();
        SetSelectedItem(null);
    }

    private void OnBasketItemAmountChanged(Item item, int amount)
    {
        if(basketItems.TryGetValue(item, out int currentAmount))
        {
            int newAmount = currentAmount + amount;
            Debug.Assert(newAmount >= 0);
            if(newAmount <= 0)
            {
                basketItems.Remove(item);
            }
            else
            {
                basketItems[item] = newAmount;
            }
        }
        else
        {
            Debug.Assert(amount > 0);
            basketItems.Add(item, amount);
        }
    }

    public void OnOpened()
    {
        Basket.ResetItems(basketItems);
        Luggage.ResetItems(NewGameManager.Instance.inventory.Items);
    }

    public void OnClosed()
    {
        CancelTransfer();
    }

    private void OnTransferLeft()
    {
        if(!selectedItem || !selectedItemIsInLuggage)
        {
            return;
        }

        ConditionallyStartTransfer();
        if (Basket.TryAddItem(selectedItem.Item))
        {
            if (!Luggage.TryRemoveItemAt(selectedItem.X, selectedItem.Y))
            {
                Debug.Log("Item added to basket could not be removed from luggage");
            }

            LogTransferChange(selectedItem.Item, -1);
            SetSelectedItem(null);
        }
    }

    private void OnTransferRight()
    {
        if(!selectedItem || selectedItemIsInLuggage)
        {
            return;
        }

        ConditionallyStartTransfer();
        if (Luggage.TryAddItem(selectedItem.Item))
        {
            if (!Basket.TryRemoveItemAt(selectedItem.X, selectedItem.Y))
            {
                Debug.Log("Item added to luggage could not be removed from basket");
            }

            LogTransferChange(selectedItem.Item, 1);
            SetSelectedItem(null);
        }
    }

    private void OnBasketItemClicked(InventorySlot slot)
    {
        SetSelectedItem(slot);
        selectedItemIsInLuggage = false;
    }

    private void OnLuggageItemClicked(InventorySlot slot)
    {
        SetSelectedItem(slot);
        selectedItemIsInLuggage = true;
    }

    private void SetSelectedItem(InventorySlot slot)
    {
        if(selectedItem)
        {
            selectedItem.SetSelected(false);
        }

        selectedItem = slot;
        if(selectedItem)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedString(selectedItem.Item.Description);
            selectedItem.SetSelected(true);
        }
        else
        {
            descriptionText.text = "";
        }
    }

    private void LogTransferChange(Item item, int amount)
    {
        if(transferChanges.TryGetValue(item, out int value))
        {
            int newValue = value + amount;
            if(newValue == 0)
            {
                transferChanges.Remove(item);
            }
            else
            {
                transferChanges[item] = newValue;
            }
        }
        else
        {
            transferChanges.Add(item, amount);
        }

        UpdateDynamics();
    }

    private void UpdateDynamics()
    {
        // Arrows
        //bool hasPositiveValues = transferChanges.Values.Any((value) => value > 0);
        //bool hasNegativeValues = transferChanges.Values.Any((value) => value < 0);

        // Money
        int price = CalculatePrice();
        moneyText.text = price.ToString();
        moneyText.gameObject.SetActive(!freeShop);

        // Accept Button
        tradeButton.enabled = CanAccept;

        // Back button
        LevelInstance.Instance.SetBackButtonVisible(CanClose);
    }

    private int CalculatePrice()
    {
        int price = 0;
        foreach(var item in transferChanges)
        {
            price += item.Key.Price * item.Value;
        }
        return price;
    }

    private void ConditionallyStartTransfer()
    {
        if(!transferInProgress)
        {
            transferInProgress = true;
            Basket.EnableGhostMode();
            Luggage.EnableGhostMode();
        }
    }

    private void StopTransfer()
    {
        if(selectedItem != null)
        {
            SetSelectedItem(null);
        }

        transferChanges.Clear();
        UpdateDynamics();
        transferInProgress = false;
    }

    private void AcceptTransfer()
    {
        if(!freeShop)
        {
            int price = CalculatePrice();
            if (price > 0 && price > NewGameManager.Instance.money)
            {
                AudioManager.Instance.PlayFX(notEnoughMoneyClip);
                return;
            }

            NewGameManager.Instance.SetMoney(NewGameManager.Instance.money - price);

            if(price > 0)
            {
                AudioManager.Instance.PlayFX(spentMoneyClip);
            }
            else if(price < 0)
            {
                AudioManager.Instance.PlayFX(receivedMoneyClip);
            }
            else
            {
                AudioManager.Instance.PlayFX(acceptTransferClip);
            }
        }
        else
        {
            AudioManager.Instance.PlayFX(acceptTransferClip);
        }

        Basket.ApplyGhostMode();
        Luggage.ApplyGhostMode();
        StopTransfer();
    }

    private void CancelTransfer()
    {
        if(transferInProgress)
        {
            Basket.CancelGhostMode();
            Luggage.CancelGhostMode();
            StopTransfer();
        }
    }

    private void OnLuggageItemAmountChanged(Item item, int changedAmount)
    {
        NewGameManager.Instance.inventory.OnItemAmountChanged(item, changedAmount);
    }

    private void OnPointerEnter(ScrollableInventoryManager manager)
    {
        HighlightedInventoryManager = manager;
    }

    private void OnPointerExit(ScrollableInventoryManager manager)
    {
        if(HighlightedInventoryManager == manager)
        {
            HighlightedInventoryManager = null;
        }
    }

    public bool OnItemDragged(DraggedItem item)
    {

        return false;
    }

    public void OnBeginDrag(DraggedItem item)
    {
        SetSelectedItem(item.Slot.InventorySlot);
    }

    public void OnDrag(DraggedItem item)
    { 
    }

    public void OnEndDrag(DraggedItem item)
    {
        if(item.IsValidTransfer)
        {
            ConditionallyStartTransfer();

            ScrollableInventoryManager sourceManager = item.Slot.InventoryManager;
            ScrollableInventoryManager targetManager = item.TargetManager;
            if (targetManager.TryAddItem(item.Slot.InventorySlot.Item))
            {
                if (!sourceManager.TryRemoveItemAt(item.Slot.InventorySlot.X, item.Slot.InventorySlot.Y))
                {
                    Debug.Log("Item added to target could not be removed from source");
                }

                int change = sourceManager == Basket ? 1 : -1;
                LogTransferChange(item.Slot.InventorySlot.Item, change);
                SetSelectedItem(null);
            }
        }
    }
}
