using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Default,
    Food
}

[CreateAssetMenu(fileName = "NewItemCategory", menuName = "ScriptableObjects/ItemCategory", order = 2)]
public class ItemCategory : ScriptableObject
{
    [Tooltip("The type of the item")]
    public ItemType type;
    [Tooltip("The conditions to add if at least one item of this category is added to the inventory. Gets removed if all items of the category are removed.")]
    public string[] SetConditions;
}
