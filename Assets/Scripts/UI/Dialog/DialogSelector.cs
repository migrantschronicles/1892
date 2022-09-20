using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSelector : DialogElement
{
    public DialogCondition Condition;

    public DialogSelector()
    {
        Type = DialogElementType.Selector;
    }
}
