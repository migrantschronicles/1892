using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField]
    private Text amountText;

    public Item Item { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Amount { get; private set; }

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetItem(Item item, int x, int y, int width, int height, int amount)
    {
        Item = item;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Amount = amount;
        image.sprite = item.sprite;
        UpdateAmountText();
    }

    public bool IsAt(int x, int y)
    {
        return x >= X && y >= Y && x < X + Width && y < Y + Height;
    }

    public bool TryAddToStack()
    {
        if(Item.IsStackable && (Item.MaxStackCount <= 0 || Amount + 1 <= Item.MaxStackCount))
        {
            ++Amount;
            UpdateAmountText();
            return true;
        }

        return false;
    }

    private void UpdateAmountText()
    {
        amountText.text = Amount.ToString();
        amountText.gameObject.SetActive(Item.IsStackable); 
    }
}
