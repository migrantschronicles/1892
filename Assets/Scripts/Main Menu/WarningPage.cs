using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WarningPage : MonoBehaviour
{
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private InputField nameField;

    private void Start()
    {
        startGameButton.enabled = false;
        nameField.onValueChanged.AddListener(OnNameChanged);
        startGameButton.onClick.AddListener(OnStartGame);
    }

    private void OnNameChanged(string name)
    {
        startGameButton.enabled = name.Length > 0;
    }

    private void OnStartGame()
    {
        SceneManager.LoadScene("Pfaffenthal");
    }
}
