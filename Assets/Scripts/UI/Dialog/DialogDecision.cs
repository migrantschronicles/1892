using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogDecision : DialogItem
{
    public SetCondition[] SetConditions;

    public DialogDecision()
    {
        Type = DialogItemType.Decision;
    }
}
