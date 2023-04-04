using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum AcceptableItemType
{
    None,
    Item,
    ItemCategory
}

[System.Serializable]
public class AcceptableItem
{
    public AcceptableItemType type = AcceptableItemType.None;
    public Item item;
    public ItemCategory category;
    public SetCondition[] setConditions;
}

/**
 * A shop. Can be a shop in the scene or a shop during a dialog.
 * Needs to be added to the LevelInstance > Canvas > Overlays game object.
 * Specify the items the shop has in the ShopItems.
 * 
 * DIALOG SHOP
 * If this is a shop to trade during a dialog (the player can give people things / accept gifts from people), set the FreeShop option.
 * This tells the shop that every trade is free and does neither cost nor gain money.
 * If it's a free shop, you can specify what items the shop (other person) can receive from the player in AcceptableItem.
 * If you set the Type to None, the player cannot give the other person anything, only receive items.
 * If you set the Type to Item, you can specify a specific item that the player can give to the person.
 * If you set the Type to ItemCategory, you can specify an item category that the player can give to the person.
 * If the player gives at least one item of the specified Item / ItemCategory, it sets the conditions that you can specify in SetConditions.
 */
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
    [SerializeField, Tooltip("The item that the shop accepts from the player. Only works if freeShop is true.")]
    private AcceptableItem acceptableItem;
    [SerializeField, Tooltip("True if this is a shop where transfering items does not cost anything (shops during dialogs)")]
    private bool freeShop;
    [SerializeField]
    private Image tradeInfo;
    [SerializeField]
    private Color defaultTradeInfoColor = new Color(0.82f, 0.8f, 0.78f, 1f);
    [SerializeField]
    private Color gainTradeInfoColor = new Color(0.61f, 0.65f, 0.47f, 1f);
    [SerializeField]
    private Color looseTradeInfoColor = new Color(0.78f, 0.38f, 0.27f, 1f);

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
    private Item providesItem;

    public delegate void OnTradeAcceptedEvent(Dictionary<Item, int> transfers);
    public event OnTradeAcceptedEvent onTradeAccepted;
    
    public ScrollableInventoryManager HighlightedInventoryManager { get; private set; }
    private bool OnlyAcceptsItem { get { return freeShop && acceptableItem.type != AcceptableItemType.None; } }
    private bool CanClose { get { return !freeShop || LevelInstance.Instance.Mode == Mode.Shop; } }

    private bool CanAccept
    {
        get
        {
            if(OnlyAcceptsItem)
            {
                return HasAcceptableItem();
            }
            else if(freeShop)
            {
                if(providesItem)
                {
                    return HasTakenItem();
                }

                return true;
            }

            int price = CalculatePrice();
            return (price < 0 || NewGameManager.Instance.money >= price);
        }
    }

    public IEnumerable<SetCondition> AcceptableItemSetConditions
    {
        get
        {
            return OnlyAcceptsItem && HasAcceptableItem() ? acceptableItem.setConditions : new SetCondition[0];
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
        Luggage.SetBagCount(NewGameManager.Instance.inventory.GetBagCount());

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

    public void InitItemAdded(Item item)
    {
        providesItem = item;
        freeShop = true;
        OnBasketItemAmountChanged(item, 1);
        UpdateDynamics();
    }

    public void InitItemRemoved(Item item)
    {
        freeShop = true;
        acceptableItem.type = AcceptableItemType.Item;
        acceptableItem.item = item;
        UpdateDynamics();
    }

    public void AddItem(Item item)
    {
        OnBasketItemAmountChanged(item, 1);
        UpdateDynamics();
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

        if(CanTransferItem(selectedItem.Item, Basket))
        {
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
    }

    private void OnTransferRight()
    {
        if(!selectedItem || selectedItemIsInLuggage)
        {
            return;
        }

        if(CanTransferItem(selectedItem.Item, Luggage))
        {
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
        moneyText.text = (-price).ToString();
        moneyText.gameObject.SetActive(!freeShop);
        tradeInfo.color = price == 0 ? defaultTradeInfoColor : (price > 0 ? looseTradeInfoColor : gainTradeInfoColor);

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

    private int GetItemsBoughtCount()
    {
        int count = 0;
        foreach(var item in transferChanges)
        {
            if(item.Value > 0)
            {
                ++count;
            }
        }

        return count;
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
            // Notify the health status.
            NewGameManager.Instance.HealthStatus.OnItemsBought(GetItemsBoughtCount());

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

        Dictionary<Item, int> transfers = new Dictionary<Item, int>(transferChanges);
        Basket.ApplyGhostMode();
        Luggage.ApplyGhostMode();
        StopTransfer();

        onTradeAccepted?.Invoke(transfers);
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
            ScrollableInventoryManager sourceManager = item.Slot.InventoryManager;
            ScrollableInventoryManager targetManager = item.TargetManager;
            if(CanTransferItem(item.Slot.InventorySlot.Item, targetManager))
            {
                ConditionallyStartTransfer();

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

    public bool CanTransferItem(Item item, ScrollableInventoryManager targetManager)
    {
        if(targetManager == Basket)
        {
            if(OnlyAcceptsItem)
            {
                switch(acceptableItem.type)
                {
                    case AcceptableItemType.None:
                        return false;

                    case AcceptableItemType.Item:
                        if(!acceptableItem.item || acceptableItem.item != item)
                        {
                            return false;
                        }
                        break;

                    case AcceptableItemType.ItemCategory:
                        if(!acceptableItem.category || acceptableItem.category != item.category)
                        {
                            return false;
                        }
                        break;
                }
            }
        }

        return true;
    }

    private bool HasAcceptableItem()
    {
        if(OnlyAcceptsItem)
        {
            switch(acceptableItem.type)
            {
                case AcceptableItemType.None:
                    return true;

                case AcceptableItemType.Item:
                    if(!acceptableItem.item)
                    {
                        return true;
                    }
                    break;

                case AcceptableItemType.ItemCategory:
                    if(!acceptableItem.category)
                    {
                        return true;
                    }
                    break;
            }

            foreach(KeyValuePair<Item, int> item in basketItems)
            { 
                switch(acceptableItem.type)
                {
                    case AcceptableItemType.Item:
                        if(item.Key == acceptableItem.item)
                        {
                            return true;
                        }
                        break;

                    case AcceptableItemType.ItemCategory:
                        if(item.Key.category == acceptableItem.category)
                        {
                            return true;
                        }
                        break;
                }
            }

            foreach(KeyValuePair<Item, int> item in transferChanges)
            {
                if(item.Value >= 0)
                {
                    continue; 
                }

                switch (acceptableItem.type)
                {
                    case AcceptableItemType.Item:
                        if (item.Key == acceptableItem.item)
                        {
                            return true;
                        }
                        break;

                    case AcceptableItemType.ItemCategory:
                        if (item.Key.category == acceptableItem.category)
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        return true;
    }

    private bool HasTakenItem()
    {
        if(freeShop && providesItem)
        {
            return Luggage.HasItem(providesItem);
        }

        return true;
    }
}
