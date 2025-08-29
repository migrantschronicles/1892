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
    V1,
    V2_DateTimeFix,
    V3_TutorialActionsAdded,
    V4_HistoryModeAdded,
    V5_CharacterAdded
}

[System.Serializable]
public class SaveData
{
    public string username;
    public float playtime = 0.0f;
    public long date;
    public int money;
    public TransportationMethod lastMethod;
    public Currency currency;
    public List<SaveDataItem> items = new();
    public List<SaveDataQuest> quests = new();
    public List<SaveDataHealthPerCharacter> health = new();
    public List<SaveDataCondition> conditions = new();
    public List<SaveDataJourney> journeys = new();
    public List<RouteManager.DiscoveredRoute> routes = new();
    public List<TutorialFeature> completedTutorialFeatures = new();
    public bool isHistoryMode;
    public CharacterType character;

    public string levelName;
    public SaveGameVersion version;

    public DateTime Date
    {
        get
        {
            return DateTime.FromFileTimeUtc(date);
        }

        set
        {
            date = value.ToFileTimeUtc();
        }
    }
}

[System.Serializable]
public class DataFile
{
    public bool hasFinishedGame = false;
}

public class SaveGameManager : MonoBehaviour
{
    public static CharacterType SaveGameCharacter { get; set; } = CharacterType.None;

    public bool SavedGameExists { get; private set; } = false;
    public DataFile DataFile { get { return dataFile; } }

    private string filePath;
    private string dataFilePath;
    private DataFile dataFile;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "savedata.json");
        dataFilePath = Path.Combine(Application.persistentDataPath, "data.json");

        if(File.Exists(dataFilePath))
        {
            string jsonData = File.ReadAllText(dataFilePath);
            dataFile = JsonUtility.FromJson<DataFile>(jsonData);
        }
        else
        {
            dataFile = new DataFile();
        }

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

        // E.g. here you could fill in other values if it's an old save game version.
        // For the dates it does not make sense to fill in other values.
        if(saveData.version < SaveGameVersion.V5_CharacterAdded)
        {
            saveData.character = CharacterType.Elis;
        }

        SaveGameCharacter = saveData.character;

        DontDestroyOnLoad(this);
        LevelManager.Instance.SelectedCharacter = saveData.character;
        SceneManager.LoadScene(saveData.levelName);
        yield return null;
        NewGameManager.Instance.LoadFromSaveGame(saveData);
        SaveGameCharacter = CharacterType.None;
        Destroy(gameObject);
    }

    public void SaveDataFile()
    {
        string jsonData = JsonUtility.ToJson(dataFile);
        File.WriteAllText(dataFilePath, jsonData);
    }

    public void OnEndGame()
    {
        File.Delete(filePath);
        SavedGameExists = false;

        dataFile.hasFinishedGame = true;
        SaveDataFile();
    }

    public void DeleteSaveGame()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        SavedGameExists = false;
    }
}
