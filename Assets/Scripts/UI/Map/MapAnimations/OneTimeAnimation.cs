using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeAnimation : MonoBehaviour
{
    public delegate void OnAnimationFinishedEvent(OneTimeAnimation anim);
    public event OnAnimationFinishedEvent OnAnimationFinished;

    public void Anim_OnFinished()
    {
        OnAnimationFinished?.Invoke(this);
    }
}
