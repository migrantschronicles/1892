using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryModeButton : MonoBehaviour
{
    [SerializeField]
    private SaveGameManager saveGameManager;

    private void Start()
    {
        GetComponent<Button>().interactable = saveGameManager.DataFile.hasFinishedGame;
    }
}
