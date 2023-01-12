using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAnimationController : MonoBehaviour
{
    protected Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetIsTalking(bool isTalking)
    {
        animator.SetBool("IsTalking", isTalking);
    }

    public void Talk()
    {
        animator.SetTrigger("Talk");
    }

    public void TalkIfNotTalking()
    {
        if(!animator.GetBool("Talk"))
        {
            Talk();
        }
    }
}
