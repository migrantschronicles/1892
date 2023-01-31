using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
