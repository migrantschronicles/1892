using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject
{
    [Tooltip("The name of the item")]
    public LocalizedString Name;
    [Tooltip("The description of the item")]
    public LocalizedString Description;
    [Tooltip("The normal sprite used in the inventory")]
    public Sprite sprite;
    [Tooltip("The sprite that should be used when it has outlines (transfered in the shop and waiting that the player accepts or rejects)")]
    public Sprite ghostSprite;
    [Tooltip("The price of the item")]
    public int Price;
    [Tooltip("The number of slots it occupies (should be 1 or 2)")]
    public int Volume = 1;
    [Tooltip("The maximum stack count. 1 for not stackable, 0 for infinite stackable.")]
    public int MaxStackCount = 1;
    [Tooltip("The conditions to add if the item is added to the inventory. Gets removed if the item is removed.")]
    public string[] SetConditions;
    [Tooltip("The category of this item")]
    public ItemCategory category;
    [Tooltip("The probability (weight) to be stolen. If an item should not be able to be stolen set it to 0." +
        "If one item has a higher change to be stolen set it to a higher value than others")]
    public float stolenProbabilityWeight = 1.0f;

    public bool IsStackable
    {
        get
        {
            return MaxStackCount != 1;
        }
    }

    public Sprite GhostSprite
    {
        get
        {
            return ghostSprite != null ? ghostSprite : sprite;
        }
    }

    public ItemType ItemType
    {
        get
        {
            return category != null ? category.type : ItemType.Default;
        }
    }
}
