using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DialogLine : DialogElement
{
    [Tooltip("The text in the bubble")]
    public LocalizedString Text;
    [Tooltip("True for npc, false for player")]
    public bool IsLeft;
    [Tooltip("Conditions that will be set when this line is played")]
    public SetCondition[] SetConditions;

    public DialogLine()
    {
        Type = DialogElementType.Line;
    }
}
