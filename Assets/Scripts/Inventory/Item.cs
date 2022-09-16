using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject
{
    [Tooltip("The name of the item")]
    public string Name;
    [Tooltip("The description of the item")]
    public string Description;
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
}
