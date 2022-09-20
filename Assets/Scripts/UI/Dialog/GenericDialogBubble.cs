using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class GenericDialogBubble : MonoBehaviour
{
    private Coroutine coroutine;
    private string fullText;

    public UnityEvent<GenericDialogBubble> onFinished = new UnityEvent<GenericDialogBubble> ();

    private IEnumerator AnimateText()
    {
        for(int i = 1; i <= fullText.Length; ++i)
        {
            SetText(fullText.Substring(0, i));
            yield return new WaitForSeconds(DialogSystem.Instance.TimeForCharacters);
        }

        onFinished.Invoke(this);
    }

    public void StartTextAnimation(string text)
    {
        fullText = text;
        coroutine = StartCoroutine(AnimateText());
    }

    public void FinishTextAnimation()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;

            SetText(fullText);
            onFinished.Invoke(this);
        }
    }

    public abstract void SetText(string value);
}
