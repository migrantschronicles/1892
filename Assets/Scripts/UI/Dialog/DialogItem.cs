using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogItemType
{
    Line,
    Decision,
    Answer,
    Selector,
    Redirector
}

public class DialogItem : MonoBehaviour
{
    public DialogItemType Type;
}
