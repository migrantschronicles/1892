using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogDecision : DialogElement
{
    [Tooltip("The conditions that will be added if one of the child decision options is selected.")]
    public SetCondition[] SetConditions;

    public DialogDecision()
    {
        Type = DialogElementType.Decision;
    }
}
