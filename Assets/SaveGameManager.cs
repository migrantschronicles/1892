using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string username;
    public DateTime date;
    public int money;

    public string levelName;

}

public class SaveGameManager : MonoBehaviour
{
    public bool SavedGameExists { get; private set; } = false;
    private SaveData saveData;

    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/savedata.json";
        saveData = new SaveData();

        if (File.Exists(filePath))
        {
            SavedGameExists = true;
        }
        else
        {
            SavedGameExists = false;
        }
    }

    public void SaveGame()
    {
        saveData.username = NewGameManager.Instance.userName;
        saveData.date = NewGameManager.Instance.date;
        saveData.money = NewGameManager.Instance.money;
        saveData.levelName = NewGameManager.Instance.nextLocation;
        Debug.Log(saveData.levelName);

        string jsonData = JsonUtility.ToJson(saveData);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        string jsonData = File.ReadAllText(filePath);
        saveData = JsonUtility.FromJson<SaveData>(jsonData);

        DontDestroyOnLoad(this);
        SceneManager.LoadScene(saveData.levelName);
        yield return null;
        NewGameManager.Instance.LoadFromSaveGame(saveData);
        Destroy(gameObject);
    }
}
