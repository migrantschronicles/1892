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

#if UNITY_EDITOR
    private void Start()
    {
        DialogSystem.ValidateChildren(new DialogElementType[]
        {
            DialogElementType.Decision,
            DialogElementType.Line,
            DialogElementType.Redirector,
            DialogElementType.Selector,
            DialogElementType.TriggerLastOption
        }, gameObject, true);
    }
#endif
}
