using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPage : MonoBehaviour
{
    [SerializeField]
    private Text dateText;
    [SerializeField]
    private Text text;

    public void SetDate(string date)
    {
        dateText.text = date;
    }

    /**
     * Sets the text of this page.
     * @return The remainig string that could not be displayed.
     */
    public string SetText(string value)
    {
        //https://docs.unity3d.com/ScriptReference/TextGenerator.html
        text.text = value;
        return value;
        //TextGenerator generator = text.cachedTextGenerator;
        //int visibleCharacterCount = generator.characterCountVisible;
        //Debug.Log(visibleCharacterCount);
        //string remainingString = visibleCharacterCount <= value.Length ? "" : value.Substring(visibleCharacterCount);
        //Debug.Log($"{visibleCharacterCount}: {text.text.Substring(0, visibleCharacterCount)} | {remainingString}");
        //return remainingString;
    }
}
