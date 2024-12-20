using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionPage : MonoBehaviour
{
    [SerializeField]
    private CharacterOption option0;
    [SerializeField]
    private CharacterOption option1;
    [SerializeField]
    private CharacterOption option2;
    [SerializeField]
    private MainMenuButton nextButton;

    private CharacterOption selectedOption;

    private void Start()
    {
        nextButton.SetEnabled(false);
        option0.onSelected += OnOptionSelected;
        option1.onSelected += OnOptionSelected;
        option2.onSelected += OnOptionSelected;
    }

    private void UnselectAll()
    {
        option0.SetSelected(false);
        option1.SetSelected(false);
        option2.SetSelected(false);
    }

    private void OnOptionSelected(CharacterOption option)
    {
        if(selectedOption != option)
        {
            UnselectAll();
            selectedOption = option;
            selectedOption.SetSelected(true);
            nextButton.SetEnabled(true);
            MainMenuController.Instance.Diary.SelectedCharacter = option.Character;
        }
    }
}
