using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveDataItem
{
    public string id;
    public int amount;
}

[System.Serializable]
public class SaveDataQuest
{
    public enum Type
    {
        Started,
        Failed,
        Finished
    }

    public string id;
    public Type type;
}

[System.Serializable]
public class SaveDataHealthPerCharacter
{
    public int requiredFoodAmount;
    public int daysWithoutEnoughFood;
    public float homesickness;
    public int homesicknessDaysSinceLastDecrease;
    public HealthStatus_Cholera.CholeraStatus choleraStatus;
    public int choleraDaysSinceExposed;
    public int choleraDaysSick;
    public bool canGetSeasick;
    public string characterName;
}

[System.Serializable]
public class SaveDataCondition
{
    public enum Type
    {
        Bool,
        Int,
        String
    }

    public Type type;
    public string name;
    public bool valueBool;
    public int valueInt;
    public string valueString;
    public bool articy;
}

[System.Serializable]
public class SaveDataJourney
{
    public string destination;
    public TransportationMethod method;
    public int money;
    public List<DiaryEntryInfo> diaryEntries;
}

public enum SaveGameVersion
{
    V1
}

[System.Serializable]
public class SaveData
{
    public string username;
    public float playtime = 0.0f;
    public DateTime date;
    public int money;
    public TransportationMethod lastMethod;
    public Currency currency;
    public List<SaveDataItem> items = new();
    public List<SaveDataQuest> quests = new();
    public List<SaveDataHealthPerCharacter> health = new();
    public List<SaveDataCondition> conditions = new();
    public List<SaveDataJourney> journeys = new();
    public List<RouteManager.DiscoveredRoute> routes = new();

    public string levelName;
    public SaveGameVersion version;
}

public class SaveGameManager : MonoBehaviour
{
    public bool SavedGameExists { get; private set; } = false;

    private string filePath;

    private void Start()
    {
        filePath = Application.persistentDataPath + "/savedata.json";

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
        SaveData saveGame = new SaveData();
        NewGameManager.Instance.SaveGame(saveGame);

        string jsonData = JsonUtility.ToJson(saveGame);
        File.WriteAllText(filePath, jsonData);
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        string jsonData = File.ReadAllText(filePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

        DontDestroyOnLoad(this);
        SceneManager.LoadScene(saveData.levelName);
        yield return null;
        NewGameManager.Instance.LoadFromSaveGame(saveData);
        Destroy(gameObject);
    }
}
