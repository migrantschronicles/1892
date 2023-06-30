using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeAnimation : MonoBehaviour
{
    public delegate void OnAnimationFinishedEvent(OneTimeAnimation anim);
    public event OnAnimationFinishedEvent OnAnimationFinished;

    private Animator animator;
    private float speed = 1.0f;

    public float Speed
    {
        get { return speed; }
        set
        {
            speed = value;
            if(animator)
            {
                animator.SetFloat("Speed", speed);
            }
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", speed);
    }

    public void Anim_OnFinished()
    {
        OnAnimationFinished?.Invoke(this);
    }
}
