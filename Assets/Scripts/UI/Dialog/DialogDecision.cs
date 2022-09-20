using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogDecision : DialogItem
{
    [Tooltip("The condition the decision sets")]
    public string SetCondition;
    [Tooltip("Whether to set it globally or only for this level")]
    public bool IsGlobal;

    public DialogDecision()
    {
        Type = DialogItemType.Decision;
    }
}
