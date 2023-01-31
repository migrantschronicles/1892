using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IAnimatedText
{
    void SetText(string text);
}

public class TextElementAnimator : ElementAnimator
{
    private static readonly float defaultTimeForCharacters = 0.1f;

    private enum TextMode { Normal, Bold, Italic }

    private IAnimatedText animatedTextInterface;
    private Text animatedText;
    private string text;
    private float timeForCharacters;
    private Coroutine coroutine;
    private MonoBehaviour owner;

    public IAnimatedText AnimatedTextInterface { get { return animatedTextInterface; } }

    public TextElementAnimator(MonoBehaviour owner, IAnimatedText animatedTextInterface, string text, float timeForCharacters)
    {
        this.animatedTextInterface = animatedTextInterface;
        this.text = text;
        this.timeForCharacters = timeForCharacters;
        this.owner = owner;
        animatedTextInterface.SetText("");
    }

    public TextElementAnimator(MonoBehaviour owner, Text animatedText, string text, float timeForCharacters)
    {
        this.animatedText = animatedText;
        this.text = text;
        this.timeForCharacters = timeForCharacters;
        this.owner = owner;
        animatedText.text = "";
    }

    public static TextElementAnimator FromText(MonoBehaviour owner, Text animatedText)
    {
        return new TextElementAnimator(owner, animatedText, animatedText.text, defaultTimeForCharacters);
    }

    private IEnumerator Animate()
    {
        // Animates the text so that the characters get displayed one after another.
        // Needs to take special care for tags e.g. "Hello <b>World</b>".

        // The text that is displayed so far.
        string displayedText = "";
        // The text that is between two tags so far.
        string modeText = "";
        // The current text mode.
        TextMode mode = TextMode.Normal;
        // The desired text mode, set when the e.g. 'b' is read from the tag.
        TextMode desiredMode = TextMode.Normal;
        // True if currently reading characters in between <...>
        bool betweenTags = false;
        for (int i = 1; i <= text.Length; ++i)
        {
            char c = text[i - 1];
            if (c == '<')
            {
                betweenTags = true;
                continue;
            }
            else if (c == '>')
            {
                betweenTags = false;
                if (mode != TextMode.Normal)
                {
                    // This is the closing tag, so reset everything.
                    mode = TextMode.Normal;
                    displayedText = text.Substring(0, i);
                    modeText = "";
                }
                else
                {
                    // This is the opening tag, so that the new mode.
                    mode = desiredMode;
                    continue;
                }
            }
            else if (betweenTags)
            {
                if (c == 'b')
                {
                    desiredMode = TextMode.Bold;
                }
                else if (c == 'i')
                {
                    desiredMode = TextMode.Italic;
                }

                continue;
            }
            else
            {
                if (mode == TextMode.Normal)
                {
                    displayedText += c;
                }
                else
                {
                    modeText += c;
                }
            }

            // Fill the full text displayed (displayedText + modeText).
            string fullText = displayedText;
            switch (mode)
            {
                case TextMode.Bold: fullText += $"<b>{modeText}</b>"; break;
                case TextMode.Italic: fullText += $"<i>{modeText}</i>"; break;
            }

            SetText(fullText);
            yield return new WaitForSeconds(timeForCharacters);
        }

        AudioManager.Instance.StopTypewriter();
        BroadcastOnFinished();
    }

    public override void Start()
    {
        coroutine = owner.StartCoroutine(Animate());
        AudioManager.Instance.StartTypewriter();
    }

    public override void Finish()
    {
        if(coroutine != null)
        {
            owner.StopCoroutine(coroutine);
            AudioManager.Instance.StopTypewriter();
        }
        SetText(text);
    }

    private void SetText(string value)
    {
        if(animatedTextInterface != null)
        {
            animatedTextInterface.SetText(value);
        }
        else if(animatedText != null)
        {
            animatedText.text = value;
        }
    }
}
