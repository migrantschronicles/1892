using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAnimationController : MonoBehaviour
{
    private Animator animator;

    public void SetIsTalking(bool isTalking)
    {
        GetAnimator().SetBool("IsTalking", isTalking);
    }

    public void Talk()
    {
        GetAnimator().SetTrigger("Talk");
    }

    public void TalkIfNotTalking()
    {
        if(!GetAnimator().GetBool("Talk"))
        {
            Talk();
        }
    }

    protected Animator GetAnimator()
    {
        if(!animator)
        {
            animator = GetComponentInChildren<Animator>();
        }

        return animator;
    }
}
