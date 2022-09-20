using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : DialogItem
{
    public DialogCondition Condition;

    public Dialog()
    {
        Type = DialogItemType.Dialog;
    }
}
