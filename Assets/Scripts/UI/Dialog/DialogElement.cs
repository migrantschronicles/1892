using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogElementType
{
    Line,
    Decision,
    DecisionOption,
    Dialog,
    Selector,
    Redirector,
    TriggerLastOption
}

public class DialogElement : MonoBehaviour
{
    public DialogElementType Type { get; protected set; }
}
