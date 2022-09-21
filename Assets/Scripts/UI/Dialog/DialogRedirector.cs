using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogRedirector : DialogElement
{
    [Tooltip("The dialog that should be played next. Does not check if the dialog meets the conditions.")]
    public Dialog Target;
    [Tooltip("True if the dialog should be added below the existing bubbles or if the existing bubbles should be removed")]
    public bool Additive = true;

    public DialogRedirector()
    {
        Type = DialogElementType.Redirector;
    }
}
