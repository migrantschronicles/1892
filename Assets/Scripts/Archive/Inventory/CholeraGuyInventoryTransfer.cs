using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CholeraGuyInventoryTransfer : MonoBehaviour
{
    static CholeraGuyInventoryTransfer()
    {
        basketInventoryIds = InventoryData.InventoryById.Where(i => i.Value.Location == InventoryLocation.CholeraGuy).Select(i => i.Key).ToList();
        luggageInventoryIds = StateManager.CurrentState.AvailableItemIds.ToList();
    }

    public CholeraGuyInventoryTransfer()
    {
        basketColums = 4;
        basketRows = 3;

        luggageColums = 4;
        luggageRows = 3;
    }

    #region Core Logic

    //Indexation
    //...
    //12 13  14 15 ...
    //8  9   10 11 ...
    //4  5   6  7  ...
    //0  1   2  3  ...

    #region UI Fields

    public List<OldInventorySlot> BasketSlots = new List<OldInventorySlot>();
    public List<OldInventorySlot> LuggageSlots = new List<OldInventorySlot>();

    public DoubleInventorySlot VerticalDoubleSlotPrefab;
    public DoubleInventorySlot HorizontalDoubleSlotPrefab;

    public RectTransform BasketParent;
    public RectTransform LuggageParent;

    public GameObject LeftArrow;
    public GameObject RightArrow;
    public GameObject CancelButton;
    public GameObject SaveButton;
    public GameObject BackButton;

    public GameObject PriceDeltaText;

    #endregion

    private int priceDelta = 0;

    private const string Basket = "Basket";
    private const string Luggage = "Luggage";

    protected int basketColums;
    protected int basketRows;

    protected int luggageColums;
    protected int luggageRows;

    public List<DoubleInventorySlot> basketDoubleSlots = new List<DoubleInventorySlot>();
    public List<DoubleInventorySlot> luggageDoubleSlots = new List<DoubleInventorySlot>();

    protected static List<int> basketInventoryIds = new List<int>();
    protected static List<int> luggageInventoryIds = new List<int>();

    protected bool considerMoneyChange = false;

    private void OnEnable()
    {
        luggageInventoryIds = StateManager.CurrentState.AvailableItemIds.ToList();
        ReorganizeClean();
    }

    void Start()
    {
        ReorganizeClean();
    }

    public void ReorganizeClean()
    {
        Reset();

        #region Init Basket

        var allDoubledItemIds = InventoryData.InventoryById.Where(i => i.Value.Volume == 2).Select(i => i.Key);

        var doubledBasketItems = allDoubledItemIds.Intersect(basketInventoryIds);
        var restBasket = basketInventoryIds.Except(doubledBasketItems);

        foreach (var doubledItem in doubledBasketItems)
        {
            bool isPositioned = false;

            for (int j = 0; j < basketRows; j++)
            {
                isPositioned = TryPositionVerticallyInBasket(j, doubledItem, Basket);

                if (!isPositioned)
                {
                    isPositioned = TryPositionHorizontallyInBasket(j, doubledItem, Basket);
                }

                if (isPositioned)
                {
                    break;
                }
            }
        }

        foreach (var id in restBasket)
        {
            foreach (var slot in BasketSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.IsEmpty = false;
                    slot.ItemId = id;
                    slot.Location = Basket;
                    slot.ItemOriginalLocation = Basket;
                    slot.Value = InventoryData.InventoryById[id].Price;
                    slot.IconKey = InventoryData.InventoryById[id].Name;
                    slot.Check();
                    break;
                }
            }
        }

        #endregion

        #region Init Luggage

        var doubledLuggageItems = allDoubledItemIds.Intersect(luggageInventoryIds);
        var restLuggage = luggageInventoryIds.Except(doubledLuggageItems);

        foreach (var doubledItem in doubledLuggageItems)
        {
            bool isPositioned = false;

            for (int j = 0; j < luggageRows; j++)
            {
                isPositioned = TryPositionVerticallyInLuggage(j, doubledItem, Luggage);

                if (!isPositioned)
                {
                    isPositioned = TryPositionHorizontallyInLuggage(j, doubledItem, Luggage);
                }

                if (isPositioned)
                {
                    break;
                }
            }
        }

        foreach (var id in restLuggage)
        {
            foreach (var slot in LuggageSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.IsEmpty = false;
                    slot.ItemId = id;
                    slot.Location = Luggage;
                    slot.ItemOriginalLocation = Luggage;
                    slot.Value = InventoryData.InventoryById[id].Price;
                    slot.IconKey = InventoryData.InventoryById[id].Name;
                    slot.Check();
                    break;
                }
            }
        }

        #endregion
    }

    private void Reset()
    {
        priceDelta = 0;

        foreach (var slot in BasketSlots)
        {
            slot.IsEmpty = true;
            slot.ItemId = null;
            slot.Type = null;
            slot.Value = 0;
            slot.Location = null;
            slot.ItemOriginalLocation = null;
            slot.IconKey = "empty";
            slot.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/empty");
            slot.Check();
        }

        foreach (var slot in LuggageSlots)
        {
            slot.IsEmpty = true;
            slot.ItemId = null;
            slot.Type = null;
            slot.Value = 0;
            slot.Location = null;
            slot.ItemOriginalLocation = null;
            slot.IconKey = "empty";
            slot.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Inventory/empty");
            slot.Check();
        }

        foreach (var doubleSlot in basketDoubleSlots.Concat(luggageDoubleSlots).ToList())
        {
            doubleSlot.ResetItem();
        }

        basketDoubleSlots.Clear();
        luggageDoubleSlots.Clear();
    }

    private void UpdateButtons()
    {
        var isBasketUpdated = BasketSlots.Concat(basketDoubleSlots).Any(i => i.Location != i.ItemOriginalLocation);
        var isLuggageUpdated = LuggageSlots.Concat(luggageDoubleSlots).Any(i => i.Location != i.ItemOriginalLocation);

        LeftArrow.SetActive(isBasketUpdated);
        RightArrow.SetActive(isLuggageUpdated);
        BackButton.SetActive(!isLuggageUpdated && !isBasketUpdated);
        SaveButton.SetActive(isLuggageUpdated || isBasketUpdated);
        CancelButton.SetActive(isLuggageUpdated || isBasketUpdated);

        if (considerMoneyChange)
        {
            priceDelta = BasketSlots.Concat(basketDoubleSlots).Where(s => !s.IsEmpty && s.ItemId.HasValue && s.ItemOriginalLocation != Basket)
                    .Sum(s => InventoryData.InventoryById[s.ItemId.Value].Price) -
                LuggageSlots.Concat(luggageDoubleSlots).Where(s => !s.IsEmpty && s.ItemId.HasValue && s.ItemOriginalLocation != Luggage)
                    .Sum(s => InventoryData.InventoryById[s.ItemId.Value].Price);

            PriceDeltaText.GetComponent<Text>().text = priceDelta.ToString();
            PriceDeltaText.SetActive(priceDelta != 0);
        }
    }

    #region Double Slot Item Positioning

    private bool TryPositionVerticallyInBasket(int rowIdex, int itemId, string originalLocation)
    {
        if (rowIdex < basketRows - 1)
        {
            for (int i = 0; i <= basketColums - 1; i++)
            {
                if (BasketSlots[i + rowIdex * basketColums].IsEmpty && BasketSlots[i + (rowIdex + 1) * basketColums].IsEmpty)
                {
                    var doubledSlot = Instantiate(VerticalDoubleSlotPrefab, Vector3.zero, Quaternion.identity);
                    doubledSlot.transform.SetParent(BasketParent);
                    doubledSlot.transform.localPosition = BasketSlots[i + rowIdex * basketColums].gameObject.transform.localPosition;

                    doubledSlot.IsEmpty = false;
                    doubledSlot.IconKey = InventoryData.InventoryById[itemId].Name;
                    doubledSlot.ItemId = itemId;
                    doubledSlot.Value = InventoryData.InventoryById[itemId].Price;
                    doubledSlot.ItemOriginalLocation = originalLocation;
                    doubledSlot.Location = Basket;
                    doubledSlot.Check();

                    doubledSlot.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        PickItem(doubledSlot);
                    });

                    doubledSlot.FirstSlot = BasketSlots[i + rowIdex * basketColums];
                    doubledSlot.SecondSlot = BasketSlots[i + +(rowIdex + 1) * basketColums];
                    BasketSlots[i + rowIdex * basketColums].IsEmpty = false;
                    BasketSlots[i + +(rowIdex + 1) * basketColums].IsEmpty = false;
                    BasketSlots[i + rowIdex * basketColums].gameObject.SetActive(false);
                    BasketSlots[i + +(rowIdex + 1) * basketColums].gameObject.SetActive(false);

                    basketDoubleSlots.Add(doubledSlot);

                    return true;
                }
            }
        }

        return false;
    }

    private bool TryPositionHorizontallyInBasket(int rowIdex, int itemId, string originalLocation)
    {
        for (int i = 0; i < basketColums - 1; i++)
        {
            if (BasketSlots[i + rowIdex * basketColums].IsEmpty && BasketSlots[i + rowIdex * basketColums + 1].IsEmpty)
            {
                var doubledSlot = Instantiate(HorizontalDoubleSlotPrefab, Vector3.zero, Quaternion.identity);
                doubledSlot.transform.SetParent(BasketParent);
                doubledSlot.transform.localPosition = BasketSlots[i + rowIdex * basketColums].gameObject.transform.localPosition;

                doubledSlot.IsEmpty = false;
                doubledSlot.IconKey = InventoryData.InventoryById[itemId].Name;
                doubledSlot.ItemId = itemId;
                doubledSlot.Value = InventoryData.InventoryById[itemId].Price;
                doubledSlot.ItemOriginalLocation = originalLocation;
                doubledSlot.Location = Basket;
                doubledSlot.Check();

                doubledSlot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PickItem(doubledSlot);
                });

                doubledSlot.FirstSlot = BasketSlots[i + rowIdex * basketColums];
                doubledSlot.SecondSlot = BasketSlots[i + rowIdex * basketColums + 1];
                BasketSlots[i + rowIdex * basketColums].IsEmpty = false;
                BasketSlots[i + rowIdex * basketColums + 1].IsEmpty = false;
                BasketSlots[i + rowIdex * basketColums].gameObject.SetActive(false);
                BasketSlots[i + rowIdex * basketColums + 1].gameObject.SetActive(false);

                basketDoubleSlots.Add(doubledSlot);

                return true;
            }
        }

        return false;
    }

    private bool TryPositionVerticallyInLuggage(int rowIdex, int itemId, string originalLocation)
    {
        if (rowIdex < luggageRows - 1)
        {
            for (int i = 0; i <= luggageColums - 1; i++)
            {
                if (LuggageSlots[i + rowIdex * luggageColums].IsEmpty && LuggageSlots[i + (rowIdex + 1) * luggageColums].IsEmpty)
                {
                    var doubledSlot = Instantiate(VerticalDoubleSlotPrefab, Vector3.zero, Quaternion.identity);
                    doubledSlot.transform.SetParent(LuggageParent);
                    doubledSlot.transform.localPosition = LuggageSlots[i + rowIdex * luggageColums].gameObject.transform.localPosition;

                    doubledSlot.IsEmpty = false;
                    doubledSlot.IconKey = InventoryData.InventoryById[itemId].Name;
                    doubledSlot.ItemId = itemId;
                    doubledSlot.Value = InventoryData.InventoryById[itemId].Price;
                    doubledSlot.ItemOriginalLocation = originalLocation;
                    doubledSlot.Location = Luggage;
                    doubledSlot.Check();

                    doubledSlot.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        PickItem(doubledSlot);
                    });

                    doubledSlot.FirstSlot = LuggageSlots[i + rowIdex * luggageColums];
                    doubledSlot.SecondSlot = LuggageSlots[i + +(rowIdex + 1) * luggageColums];
                    LuggageSlots[i + rowIdex * luggageColums].IsEmpty = false;
                    LuggageSlots[i + +(rowIdex + 1) * luggageColums].IsEmpty = false;
                    LuggageSlots[i + rowIdex * luggageColums].gameObject.SetActive(false);
                    LuggageSlots[i + +(rowIdex + 1) * luggageColums].gameObject.SetActive(false);

                    luggageDoubleSlots.Add(doubledSlot);

                    return true;
                }
            }
        }

        return false;
    }

    private bool TryPositionHorizontallyInLuggage(int rowIdex, int itemId, string originalLocation)
    {
        for (int i = 0; i < luggageColums - 1; i++)
        {
            if (LuggageSlots[i + rowIdex * luggageColums].IsEmpty && LuggageSlots[i + rowIdex * luggageColums + 1].IsEmpty)
            {
                var doubledSlot = Instantiate(HorizontalDoubleSlotPrefab, Vector3.zero, Quaternion.identity);
                doubledSlot.transform.SetParent(LuggageParent);
                doubledSlot.transform.localPosition = LuggageSlots[i + rowIdex * luggageColums].gameObject.transform.localPosition;

                doubledSlot.IsEmpty = false;
                doubledSlot.IconKey = InventoryData.InventoryById[itemId].Name;
                doubledSlot.ItemId = itemId;
                doubledSlot.Value = InventoryData.InventoryById[itemId].Price;
                doubledSlot.ItemOriginalLocation = originalLocation;
                doubledSlot.Location = Luggage;
                doubledSlot.Check();

                doubledSlot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PickItem(doubledSlot);
                });

                doubledSlot.FirstSlot = LuggageSlots[i + rowIdex * luggageColums];
                doubledSlot.SecondSlot = LuggageSlots[i + rowIdex * luggageColums + 1];
                LuggageSlots[i + rowIdex * luggageColums].IsEmpty = false;
                LuggageSlots[i + rowIdex * luggageColums + 1].IsEmpty = false;
                LuggageSlots[i + rowIdex * luggageColums].gameObject.SetActive(false);
                LuggageSlots[i + rowIdex * luggageColums + 1].gameObject.SetActive(false);

                luggageDoubleSlots.Add(doubledSlot);

                return true;
            }
        }

        return false;
    }

    #endregion

    #region Transfering Logic

    public DoubleInventorySlot TransferItem(DoubleInventorySlot item)
    {
        DoubleInventorySlot updatedSlot = null;

        if (item.Location == Luggage)
        {
            for (int j = 0; j < basketRows; j++)
            {
                if (!item.ItemId.HasValue)
                {
                    break;
                }

                if (!TryPositionVerticallyInBasket(j, item.ItemId.Value, item.ItemOriginalLocation))
                {
                    if (TryPositionHorizontallyInBasket(j, item.ItemId.Value, item.ItemOriginalLocation))
                    {
                        luggageDoubleSlots.Remove(item);
                        item.ResetItem();
                    }
                }
                else
                {
                    luggageDoubleSlots.Remove(item);
                    item.ResetItem();
                }
            }
        }
        else if (item.Location == Basket)
        {
            for (int j = 0; j < luggageRows; j++)
            {
                if (!item.ItemId.HasValue)
                {
                    break;
                }

                if (!TryPositionVerticallyInLuggage(j, item.ItemId.Value, item.ItemOriginalLocation))
                {
                    if (TryPositionHorizontallyInLuggage(j, item.ItemId.Value, item.ItemOriginalLocation))
                    {
                        basketDoubleSlots.Remove(item);
                        item.ResetItem();
                    }
                }
                else
                {
                    basketDoubleSlots.Remove(item);
                    item.ResetItem();
                }
            }
        }

        return updatedSlot;
    }

    public OldInventorySlot TransferItem(OldInventorySlot item)
    {
        if (item.IsEmpty)
        {
            return null;
        }

        OldInventorySlot updatedSlot = null;

        if (item.Location == Luggage)
        {
            foreach (var slot in BasketSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.GetComponent<Image>().sprite = item.ItemOriginalLocation == Basket ?
                        Resources.Load<Sprite>($"Inventory/{item.IconKey}") :
                        Resources.Load<Sprite>($"Inventory/Highlights/{item.IconKey}");

                    slot.IconKey = item.IconKey;
                    slot.ItemId = item.ItemId;
                    slot.Location = Basket;
                    slot.ItemOriginalLocation = item.ItemOriginalLocation;
                    slot.Value = item.Value;
                    slot.Type = item.Type;
                    slot.IsEmpty = false;

                    updatedSlot = slot;
                    item.ResetItem();

                    break;
                }
            }
        }
        else if (item.Location == Basket)
        {
            foreach (var slot in LuggageSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.GetComponent<Image>().sprite = item.ItemOriginalLocation == Luggage ?
                        Resources.Load<Sprite>($"Inventory/{item.IconKey}") :
                        Resources.Load<Sprite>($"Inventory/Highlights/{item.IconKey}");

                    slot.IconKey = item.IconKey;
                    slot.ItemId = item.ItemId;
                    slot.Location = Luggage;
                    slot.ItemOriginalLocation = item.ItemOriginalLocation;
                    slot.Value = item.Value;
                    slot.Type = item.Type;
                    slot.IsEmpty = false;

                    updatedSlot = slot;
                    item.ResetItem();

                    break;
                }
            }
        }

        return updatedSlot;
    }

    public void PickItem(OldInventorySlot item)
    {
        if (!item.IsEmpty)
        {
            TransferItem(item);
            UpdateButtons();
        }
    }

    public void PickItem(DoubleInventorySlot item)
    {
        if (!item.IsEmpty)
        {
            TransferItem(item);
            UpdateButtons();
        }
    }

    #endregion

    #region Save/Cancel

    public void SaveChanges()
    {
        if (StateManager.CurrentState.AvailableMoney + priceDelta < 0)
        {
            return;
        }

        if (considerMoneyChange)
        {
            StateManager.CurrentState.AvailableMoney += priceDelta;
            priceDelta = 0;
        }

        StateManager.CurrentState.AvailableItemIds = LuggageSlots.Concat(luggageDoubleSlots).Where(s => !s.IsEmpty && s.ItemId.HasValue).Select(s => s.ItemId.Value);
        luggageInventoryIds = LuggageSlots.Concat(luggageDoubleSlots).Where(s => !s.IsEmpty && s.ItemId.HasValue).Select(s => s.ItemId.Value).ToList();
        basketInventoryIds = BasketSlots.Concat(basketDoubleSlots).Where(s => !s.IsEmpty && s.ItemId.HasValue).Select(s => s.ItemId.Value).ToList();

        ReorganizeClean();
        UpdateButtons();
    }

    public void CancelChanges()
    {
        ReorganizeClean();
        UpdateButtons();
    }

    #endregion

    #endregion
}
