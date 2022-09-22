using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialog : DialogElement
{
    [Tooltip("This dialog only plays if this condition is met")]
    public DialogCondition Condition;
    [Tooltip("Event that is called when the dialog finishes, either by playing all dialog lines or by leaving the dialog via redirector.")]
    public UnityEvent OnFinished = new UnityEvent();

    public Dialog()
    {
        Type = DialogElementType.Dialog;
    }
}
