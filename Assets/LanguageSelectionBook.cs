using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelectionBook : MonoBehaviour
{

    public string language;
    public MainMenu menu;

    private void OnMouseDown()
    {
        GetComponentInChildren<Animator>().SetTrigger("Language selection 0");
        menu.SelectLanguage(language);
    }

}
