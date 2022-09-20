using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogAnswer : DialogItem
{
    [Tooltip("The condition has to be met so that the answer option is displayed")]
    public DialogCondition Condition;
    [Tooltip("The condition that will be set if this option is chosen")]
    public string SetCondition;
    [Tooltip("Whether to set the condition globally or only for this level")]
    public bool IsGlobal;
    [Tooltip("The answer type")]
    public AnswerType AnswerType;
    [Tooltip("The text to display")]
    public string Text;

    public DialogAnswer()
    {
        Type = DialogItemType.Answer;
    }
}
