using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageElementAnimator : ElementAnimator
{
    private static readonly float defaultAnimationTime = 1.5f;

    private Image target;
    private MonoBehaviour owner;
    private Coroutine coroutine;
    private float targetAlpha;
    private float animationTime;
    private float currentTime = 0.0f;

    public ImageElementAnimator(MonoBehaviour owner, Image target, float targetAlpha, float animationTime)
    {
        this.owner = owner;
        this.target = target;
        this.targetAlpha = targetAlpha;
        this.animationTime = animationTime;
        SetAlpha(0.0f);
    }

    public static ImageElementAnimator FromImage(MonoBehaviour owner, Image target)
    {
        return new ImageElementAnimator(owner, target, target.color.a, defaultAnimationTime);
    }

    private IEnumerator Animate()
    {
        while(currentTime < animationTime)
        {
            currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0.0f, animationTime);
            float value = targetAlpha * (currentTime / animationTime);
            SetAlpha(value);
            yield return new WaitForSeconds(0.0f);
        }

        BroadcastOnFinished();
    }

    public override void Finish()
    {
        if(coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
        SetAlpha(targetAlpha);
    }

    public override void Start()
    {
        coroutine = owner.StartCoroutine(Animate());
    }

    private void SetAlpha(float alpha)
    {
        target.color = new Color(target.color.r, target.color.g, target.color.b, alpha);
    }
}
