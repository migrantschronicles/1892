using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : DialogElement
{
    public DialogCondition Condition;

    public Dialog()
    {
        Type = DialogElementType.Dialog;
    }
}
