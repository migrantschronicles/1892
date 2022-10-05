using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimatedText
{
    void SetText(string text);
}

public class TextElementAnimator : ElementAnimator
{
    private enum TextMode { Normal, Bold, Italic }

    private IAnimatedText animatedText;
    private string text;
    private float timeForCharacters;
    private Coroutine coroutine;
    private MonoBehaviour owner;

    public TextElementAnimator(MonoBehaviour owner, IAnimatedText animatedText, string text, float timeForCharacters)
    {
        this.animatedText = animatedText;
        this.text = text;
        this.timeForCharacters = timeForCharacters;
        this.owner = owner;
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

            animatedText.SetText(fullText);
            yield return new WaitForSeconds(timeForCharacters);
        }

        onFinished.Invoke(this);
    }

    public override void Start()
    {
        coroutine = owner.StartCoroutine(Animate());
    }

    public override void Finish()
    {
        owner.StopCoroutine(coroutine);
        animatedText.SetText(text);
    }
}
