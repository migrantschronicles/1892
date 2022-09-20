using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogAnswer : DialogElement
{
    [Tooltip("The condition has to be met so that the answer option is displayed")]
    public DialogCondition Condition;
    [Tooltip("The condition that will be set if this option is chosen")]
    public SetCondition[] SetConditions;
    [Tooltip("The answer type")]
    public AnswerType AnswerType;
    [Tooltip("The text to display")]
    public string Text;

    public DialogAnswer()
    {
        Type = DialogElementType.Answer;
    }
}
