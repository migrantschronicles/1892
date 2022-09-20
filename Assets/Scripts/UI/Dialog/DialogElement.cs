using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogElementType
{
    Line,
    Decision,
    Answer,
    Dialog,
    Selector,
    Redirector,
}

public class DialogElement : MonoBehaviour
{
    public DialogElementType Type { get; protected set; }
}
