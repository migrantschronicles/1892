using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSelector : DialogElement
{
    [Tooltip("The condition that must be met so that the children of this selector are played.")]
    public DialogCondition Condition;

    public DialogSelector()
    {
        Type = DialogElementType.Selector;
    }
}
