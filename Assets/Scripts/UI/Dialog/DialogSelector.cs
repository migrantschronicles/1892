using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSelector : DialogItem
{
    public DialogCondition Condition;

    public DialogSelector()
    {
        Type = DialogItemType.Selector;
    }
}
