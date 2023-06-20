using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageBookAnimReceiver : MonoBehaviour
{
    private void OnSelectionAnimStarted()
    {
        GetComponentInParent<BoxCollider2D>().enabled = false;
    }

    private void OnSelectionAnimFinished()
    {
        GetComponentInParent<BoxCollider2D>().enabled = true;
    }
}
