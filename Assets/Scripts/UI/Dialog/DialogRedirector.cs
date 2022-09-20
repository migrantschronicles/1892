using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogRedirector : DialogElement
{
    public Dialog Target;
    public bool Additive = true;

    public DialogRedirector()
    {
        Type = DialogElementType.Redirector;
    }
}
