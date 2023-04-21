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
    public int food;

    public string levelName;

}

public class SaveGameManager : MonoBehaviour
{
    public bool savedGameExists = false;
    private SaveData saveData;

    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/savedata.json";
        saveData = new SaveData();

        if (File.Exists(filePath))
        {
            savedGameExists = true;
        }
        else
        {
            savedGameExists = false;
        }
    }

    public void SaveGame()
    {
        saveData.username = NewGameManager.Instance.userName;
        saveData.date = NewGameManager.Instance.date;
        saveData.money = NewGameManager.Instance.money;
        //saveData.food = NewGameManager.Instance.food;
        saveData.levelName = NewGameManager.Instance.nextLocation;
        Debug.Log(saveData.levelName);

        string jsonData = JsonUtility.ToJson(saveData);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        string jsonData = File.ReadAllText(filePath);
        saveData = JsonUtility.FromJson<SaveData>(jsonData);


        // Error happening: Should load the gamemanager after the load scene
        /*NewGameManager.Instance.username = saveData.username;
         * NewGameManager.Instance.date = saveData.date;
        NewGameManager.Instance.money = saveData.money;*/
        //NewGameManager.Instance.food = saveData.food;

        SceneManager.LoadScene(saveData.levelName);
        Debug.Log("Game Loaded");
        
    }
}
