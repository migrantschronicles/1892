using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public UnityEvent<InventorySlot> OnClicked = new UnityEvent<InventorySlot>();

    [SerializeField]
    private Text amountText;
    [SerializeField]
    private Color defaultTextColor;
    [SerializeField]
    private Color ghostTextColor;

    public Item Item { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Amount { get; private set; }
    public InventoryManager Manager { get; set; }

    private Image image;
    /// Value is updated to Amount if not ghost. If ghost, value represents the Amount + added during ghost, Amount still represents amount in inventory.
    private int ghostAmount = 0;
    private bool isSelected = false;
    private bool isGhostMode = false;
    private Material material;

    public int ChangedAmount
    {
        get
        {
            return ghostAmount - Amount;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        material = Instantiate(image.material);
        image.material = material;
    }

    private bool TryAddAmount(bool ghost)
    {
        int currentAmount = ghost ? ghostAmount : Amount;
        if(currentAmount == 0 || (Item.IsStackable && (Item.MaxStackCount <= 0 || currentAmount + 1 <= Item.MaxStackCount)))
        {
            if(ghost)
            {
                ++ghostAmount;
            }
            else
            {
                ++Amount;
                ghostAmount = Amount;
            }

            return true;
        }

        return false;
    }

    private bool TryRemoveAmount(bool ghost)
    {
        int currentAmount = ghost ? ghostAmount : Amount;

        if(ghost)
        {
            --ghostAmount;
        }
        else
        {
            --Amount;
            ghostAmount = Amount;
        }

        if (!Item.IsStackable || currentAmount <= 1)
        {
            return false;
        }

        return true;
    }

    public void SetItem(Item item, int x, int y, int width, int height, bool ghost)
    {
        Item = item;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        isGhostMode = ghost;
        image.sprite = item.sprite;
        TryAddAmount(ghost);
        UpdateAmountText();
        UpdateVisuals();
    }

    public bool IsAt(int x, int y)
    {
        return x >= X && y >= Y && x < X + Width && y < Y + Height;
    }

    public bool TryAddToStack(bool ghost)
    {
        if(TryAddAmount(ghost))
        {
            amountText.color = ghost ? ghostTextColor : defaultTextColor;
            UpdateAmountText();
            return true;
        }

        return false;
    }

    public bool TryRemoveFromStack(bool ghost)
    {
        if(TryRemoveAmount(ghost))
        {
            amountText.color = ghost ? ghostTextColor : defaultTextColor;
            UpdateAmountText();
            return true;
        }

        return false;
    }

    private void UpdateAmountText()
    {
        amountText.text = ghostAmount.ToString();
        amountText.gameObject.SetActive(Item.IsStackable); 
    }

    public void EnableGhostMode()
    {
        isGhostMode = true;
        UpdateVisuals();
    }

    public InventoryGhostChange ApplyGhostMode()
    {
        InventoryGhostChange change = InventoryGhostChange.None;
        if(Amount == 0)
        {
            change = InventoryGhostChange.Item;
        }
        else if(Amount != ghostAmount)
        {
            change = InventoryGhostChange.Amount;
        }

        isGhostMode = false;
        Amount = ghostAmount;
        UpdateAmountText();
        UpdateVisuals();

        return change;
    }

    public InventoryGhostChange CancelGhostMode()
    {
        InventoryGhostChange change = InventoryGhostChange.None;
        if(Amount == 0)
        {
            change = InventoryGhostChange.Item;
        }

        // Reset the displayed amount.
        isGhostMode = false;
        ghostAmount = Amount;
        UpdateAmountText();
        UpdateVisuals();

        return change;
    }

    public void OnSlotClicked()
    {
        OnClicked.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        bool defaultVisuals = !(isSelected || isGhostMode);
        if(defaultVisuals)
        {
            image.materialForRendering.SetFloat("_OutlineEnabled", 0.0f);
        }
        else
        {
            image.materialForRendering.SetFloat("_OutlineColorIndex", isSelected ? 1 : 0);
            image.materialForRendering.SetFloat("_OutlineEnabled", 1.0f);
        }
        amountText.color = defaultVisuals ? defaultTextColor : ghostTextColor;
    }
}
