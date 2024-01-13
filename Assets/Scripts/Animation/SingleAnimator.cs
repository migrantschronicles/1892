using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleAnimator : MonoBehaviour
{
    [SerializeField]
    private float animationSpeed = 1.0f;
    [SerializeField]
    private float offset = 0.0f;

    private void Start()
    {
        Animator anim = GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetFloat("AnimationSpeed", animationSpeed);
            anim.SetFloat("Offset", offset);
        }
    }
}
