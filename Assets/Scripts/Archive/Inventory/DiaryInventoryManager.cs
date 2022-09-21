using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DiaryInventoryManager : MonoBehaviour
{
    #region Inventory Management

    public Text SelectionDescription;

    public List<OldInventorySlot> LuggageSlots = new List<OldInventorySlot>();

    public DoubleInventorySlot VerticalDoubleSlotPrefab;
    public DoubleInventorySlot HorizontalDoubleSlotPrefab;

    public RectTransform LuggageParent;

    protected int luggageColums = 4;
    protected int luggageRows = 3;

    private List<DoubleInventorySlot> luggageDoubleSlots = new List<DoubleInventorySlot>();

    private List<int> luggageInventoryIds = StateManager.CurrentState?.AvailableItemIds?.ToList();

    private const string Luggage = "Luggage";

    private OldInventorySlot currentSelection;
    public OldInventorySlot CurrentSelection
    {
        get => currentSelection;
        set
        {
            if (currentSelection != value && value != null)
            {
                if (currentSelection != null)
                {
                    currentSelection.IsSelected = false;
                    currentSelection.Check();
                }

                currentSelection = value;
                currentSelection.IsSelected = true;

                if (currentSelection is DoubleInventorySlot doubleSlot)
                {
                    doubleSlot.Check();
                }
                else
                {
                    currentSelection.Check();
                }

                if (currentSelection.ItemId.HasValue)
                {
                    SelectionDescription.GetComponent<Text>().text = InventoryData.DescriptionById[currentSelection.ItemId.Value];
                }
            }
        }
    }

    private void OnEnable()
    {
        luggageInventoryIds = StateManager.CurrentState.AvailableItemIds.ToList();

        Reset();

        var allDoubledItemIds = InventoryData.InventoryById.Where(i => i.Value.Volume == 2).Select(i => i.Key);
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
                    slot.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        CurrentSelection = slot;
                    });
                    break;
                }
            }
        }

        if (luggageDoubleSlots.Any())
        {
            CurrentSelection = luggageDoubleSlots.First();
        }
        else
        {
            LuggageSlots.FirstOrDefault(s => !s.IsEmpty && s.ItemId.HasValue);
        }
    }

    private void Reset()
    {
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

        foreach (var doubleSlot in luggageDoubleSlots.ToList())
        {
            doubleSlot.ResetItem();
        }

        luggageDoubleSlots.Clear();
    }

    public void RemoveSelection()
    {
        if (CurrentSelection != null && CurrentSelection.ItemId.HasValue)
        {
            var list = StateManager.CurrentState.AvailableItemIds.ToList();
            list.Remove(CurrentSelection.ItemId.Value);
            StateManager.CurrentState.AvailableItemIds = list;

            if (CurrentSelection is DoubleInventorySlot doubleSlot)
            {
                doubleSlot.ResetItem();
                doubleSlot.Check();
            }
            else
            {
                CurrentSelection.ResetItem();
                CurrentSelection.Check();
            }

            currentSelection = null;
        }

        SelectionDescription.GetComponent<Text>().text = string.Empty;
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
                        CurrentSelection = doubledSlot;
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
                    CurrentSelection = doubledSlot;
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


    void Update()
    {

    }

    #endregion

    #region Paging

    public GameObject InventoryPage;
    public GameObject DiaryPage;
    public GameObject HealthPage;
    public GameObject SettingPage;
    public GameObject MapPage;

    public void DisplayInventoryPage()
    {
        InventoryPage.SetActive(true);
        DiaryPage.SetActive(false);
        HealthPage.SetActive(false);
        SettingPage.SetActive(false);
        MapPage.SetActive(false);
    }

    public void DisplayDiaryPage()
    {
        InventoryPage.SetActive(false);
        DiaryPage.SetActive(true);
        HealthPage.SetActive(false);
        SettingPage.SetActive(false);
        MapPage.SetActive(false);
    }

    public void DisplayHealthPage()
    {
        InventoryPage.SetActive(false);
        DiaryPage.SetActive(false);
        HealthPage.SetActive(true);
        SettingPage.SetActive(false);
        MapPage.SetActive(false);
    }

    public void DisplaySettingPage()
    {
        InventoryPage.SetActive(false);
        DiaryPage.SetActive(false);
        HealthPage.SetActive(false);
        SettingPage.SetActive(true);
        MapPage.SetActive(false);
    }

    public void DisplayMapPage()
    {
        InventoryPage.SetActive(false);
        DiaryPage.SetActive(false);
        HealthPage.SetActive(false);
        SettingPage.SetActive(false);
        MapPage.SetActive(true);
    }

    #endregion
}
