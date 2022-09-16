using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
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

    private Image image;
    /// Value is updated to Amount if not ghost. If ghost, value represents the Amount + added during ghost, Amount still represents amount in inventory.
    private int ghostAmount = 0;

    private void Awake()
    {
        image = GetComponent<Image>();
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

    public void SetItem(Item item, int x, int y, int width, int height, bool ghost)
    {
        Item = item;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        image.sprite = ghost ? item.GhostSprite : item.sprite;
        TryAddAmount(ghost);
        amountText.color = ghost ? ghostTextColor : defaultTextColor;

        UpdateAmountText();
    }

    public bool IsAt(int x, int y)
    {
        return x >= X && y >= Y && x < X + Width && y < Y + Height;
    }

    public bool TryAddToStack(bool ghost)
    {
        if(Item.IsStackable && (Item.MaxStackCount <= 0 || ghostAmount + 1 <= Item.MaxStackCount))
        {
            TryAddAmount(ghost);
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

        Amount = ghostAmount;
        UpdateAmountText();
        amountText.color = defaultTextColor;
        image.sprite = Item.sprite;

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
        ghostAmount = Amount;
        UpdateAmountText();
        amountText.color = defaultTextColor;

        return change;
    }
}
