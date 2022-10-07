using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTriggerLastOption : DialogElement
{
    public DialogTriggerLastOption()
    {
        Type = DialogElementType.TriggerLastOption;
    }

#if UNITY_EDITOR
    private void Start()
    {
        DialogSystem.ValidateChildren(null, gameObject);

        DialogElement parent = this;
        do
        {
            parent = parent.transform.parent.GetComponent<DialogElement>();
            if(parent != null)
            {
                if(parent.Type == DialogElementType.DecisionOption)
                {
                    break;
                }
                else if(parent.Type == DialogElementType.Selector)
                {
                    continue;
                }
                else
                {
                    DialogSystem.LogValidateError($"A DialogTriggerLastOption may only be placed right below a decision option " +
                        $"or beneath a selector (or multiple selectors) which are directly beneath a decision option, not {parent.name}", gameObject);
                    break;
                }
            }
        }
        while (true);
    }
#endif
}
