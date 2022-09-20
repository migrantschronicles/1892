using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogDecision : DialogElement
{
    public SetCondition[] SetConditions;

    public DialogDecision()
    {
        Type = DialogElementType.Decision;
    }
}
