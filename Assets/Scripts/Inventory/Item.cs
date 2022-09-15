using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite sprite;
    public int Price;
    public int Volume = 1;
    public int MaxStackCount = 1;

    public bool IsStackable
    {
        get
        {
            return MaxStackCount != 1;
        }
    }
}
