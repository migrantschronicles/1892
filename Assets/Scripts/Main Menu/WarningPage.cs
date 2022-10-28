using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WarningPage : MonoBehaviour
{
    [SerializeField]
    private MainMenuButton startGameButton;
    [SerializeField]
    private InputField nameField;

    private void Start()
    {
        startGameButton.SetEnabled(false);
        nameField.onValueChanged.AddListener(OnNameChanged);
        startGameButton.onClick += OnStartGame;
    }

    private void OnNameChanged(string name)
    {
        startGameButton.SetEnabled(name.Length > 0);
    }

    private void OnStartGame()
    {
        SceneManager.LoadScene("Pfaffenthal");
    }
}
