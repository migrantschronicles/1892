using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogRedirector : DialogItem
{
    public Dialog Target;
    public bool Additive = true;

    public DialogRedirector()
    {
        Type = DialogItemType.Redirector;
    }
}
