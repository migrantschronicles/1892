using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelectionBook : MonoBehaviour
{

    [SerializeField]
    private Language language;

    private void OnMouseDown()
    {
        GetComponentInChildren<Animator>().SetTrigger("Language selection 0");
        MainMenuController.Instance.OnLanguageSelected(language);
    }
}
