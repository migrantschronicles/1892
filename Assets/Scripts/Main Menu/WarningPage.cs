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
        LevelManager.Instance.StartNewGame(nameField.text);
    }

    public void OpenMoreInfo()
    {
        Application.OpenURL("https://www.iom.int/sites/g/files/tmzbdl486/files/our_work/DMM/Migration-Health/MP_infosheets/MHPSS-refugees-asylum-seekers-migrants-Europe-Multi-Agency-guidance-note.pdf");
    }
}
