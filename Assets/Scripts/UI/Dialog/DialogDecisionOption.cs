using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DialogDecisionOption : DialogElement
{
    [Tooltip("The condition has to be met so that the answer option is displayed")]
    public DialogCondition Condition;
    [Tooltip("The condition has to be met so that the answer option is activated, not deactived / disabled")]
    public DialogCondition EnabledCondition;
    [Tooltip("The condition that will be set if this option is chosen")]
    public SetCondition[] SetConditions;
    [Tooltip("The answer type")]
    public AnswerType AnswerType;
    [Tooltip("The text to display")]
    public LocalizedString Text;
    [Tooltip("Trigger the action (e.g. open inventory to trade items) automatically or use DialogTriggerLastOption manually.")]
    public bool autoTriggerAction = true;
    [Tooltip("The shop to open for Quest and Items answer types")]
    public Shop shop;

    public DialogDecisionOption()
    {
        Type = DialogElementType.DecisionOption;
    }

#if UNITY_EDITOR
    private void Start()
    {
        DialogSystem.ValidateSetConditions(SetConditions, gameObject);

        if(Text.IsEmpty)
        {
            DialogSystem.LogValidateError("Text is empty", gameObject);
        }

        switch(AnswerType)
        {
            case AnswerType.Items:
            case AnswerType.Quest:
                if(shop == null)
                {
                    DialogSystem.LogValidateError("The shop needs to be set", gameObject);
                }
                break;
        }

        DialogSystem.ValidateChildren(new DialogElementType[]
        {
            DialogElementType.Decision,
            DialogElementType.Line,
            DialogElementType.Redirector,
            DialogElementType.Selector,
            DialogElementType.TriggerLastOption
        }, gameObject);
    }
#endif
}
