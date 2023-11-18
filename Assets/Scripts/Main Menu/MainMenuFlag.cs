using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuFlag : MonoBehaviour
{
    public Image Flag;
    public Language language;

    private void OnEnable()
    {
        Flag.enabled = LocalizationManager.Instance.CurrentLanguage == language;
    }
}
