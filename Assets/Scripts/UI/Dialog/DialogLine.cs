using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogLine : DialogItem
{
    public string Text;
    public bool IsLeft;
    public SetCondition[] SetConditions;

    public DialogLine()
    {
        Type = DialogItemType.Line;
    }
}
