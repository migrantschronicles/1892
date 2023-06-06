using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Animates elements in the diary or in dialogs.
 * This can be a TextElementAnimator to animate text, or ImageElementAnimator to fade in images.
 */
public abstract class ElementAnimator
{
    public delegate void OnFinishedDelegate(ElementAnimator animator);
    public event OnFinishedDelegate onFinished;

    public abstract void Start();
    public abstract void Finish();

    protected void BroadcastOnFinished()
    {
        onFinished?.Invoke(this);
    }
}
