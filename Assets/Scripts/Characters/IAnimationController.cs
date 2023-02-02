using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAnimationController : MonoBehaviour
{
    [SerializeField]
    private float neutralSpeed = 1.0f;
    [SerializeField]
    private float talkSpeed = 1.0f;

    private Animator animator;
    private bool isTalking = false;

    private void OnEnable()
    {
        GetAnimator().SetBool("IsTalking", isTalking);
        GetAnimator().SetFloat("NeutralSpeed", neutralSpeed);
        GetAnimator().SetFloat("TalkSpeed", talkSpeed);
    }

    public void SetIsTalking(bool isTalking)
    {
        GetAnimator().SetBool("IsTalking", isTalking);
        this.isTalking = isTalking;
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