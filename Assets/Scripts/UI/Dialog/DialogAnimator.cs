using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogAnimator
{
    public delegate void OnFinishedDelegate(DialogAnimator animator);
    public OnFinishedDelegate OnFinished;

    public abstract void Start();
    public abstract void Finish();
}

public class DialogTextAnimator : DialogAnimator
{
    private IDialogBubble bubble;
    private string text;
    private float timeForCharacters;
    private Coroutine coroutine;
    private MonoBehaviour target;

    public DialogTextAnimator(MonoBehaviour target, IDialogBubble bubble, string text, float timeForCharacters)
    {
        this.bubble = bubble;
        this.text = text;
        this.timeForCharacters = timeForCharacters;
        this.target = target;
    }

    private IEnumerator Animate()
    {
        for(int i = 1; i <= text.Length; ++i)
        {
            bubble.SetText(text.Substring(0, i));
            yield return new WaitForSeconds(timeForCharacters);
        }

        OnFinished.Invoke(this);
    }

    public override void Start()
    {
        coroutine = target.StartCoroutine(Animate());
    }

    public override void Finish()
    {
        target.StopCoroutine(coroutine);
        bubble.SetText(text);
    }
}
