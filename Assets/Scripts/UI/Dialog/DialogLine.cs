using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogLine : DialogElement
{
    public string Text;
    public bool IsLeft;
    public SetCondition[] SetConditions;

    public DialogLine()
    {
        Type = DialogElementType.Line;
    }
}
