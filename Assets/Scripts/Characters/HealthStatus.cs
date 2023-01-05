using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfDayHealthData
{
    /// The name of the character.
    public string name;
    /// The amount of food he received.
    public int foodAmount;
}

public class HealthStatus_Hungry
{
    // Stored so we can retrieve if the character has cholera (double food amount).
    private CharacterHealthStatus status;
    private int requiredFoodAmount = 0;

    public HealthStatus_Hungry(CharacterHealthStatus status)
    {
        this.status = status;
    }

    public void OnEndOfDay(int receivedFoodAmount)
    {
        requiredFoodAmount = requiredFoodAmount * 2 + 1;
        requiredFoodAmount = Mathf.Max(0, requiredFoodAmount - receivedFoodAmount);
    }
}

public class CharacterHealthStatus
{
    private HealthStatus_Hungry hungryStatus;

    public CharacterHealthData CharacterData { get; private set; }

    public CharacterHealthStatus()
    {
        hungryStatus = new HealthStatus_Hungry(this);
    }

    public void Init(CharacterHealthData characterData)
    {
        CharacterData = characterData;
    }

    public void OnEndOfDay(EndOfDayHealthData healthData)
    {
        hungryStatus.OnEndOfDay(healthData != null ? healthData.foodAmount : 0);
    }
}

public class HealthStatus
{
    private List<CharacterHealthStatus> characters = new List<CharacterHealthStatus>();

    public void Init(IEnumerable<CharacterHealthData> characterData)
    {
        foreach(CharacterHealthData character in characterData)
        {
            CharacterHealthStatus status = new CharacterHealthStatus();
            status.Init(character);
            characters.Add(status);
        }
    }

    public void OnEndOfDay(IEnumerable<EndOfDayHealthData> data)
    {
        // Pass the callback to the health status.
        List<string> handledCharacters = new List<string>();
        foreach(EndOfDayHealthData healthData in data)
        {
            CharacterHealthStatus status = GetHealthStatus(healthData.name);
            if(status == null)
            {
                Debug.LogError($"Character {healthData.name} does not exist in OnEndOfDay");
                continue;
            }

            status.OnEndOfDay(healthData);
        }

        if(handledCharacters.Count < characters.Count)
        {
            // Go through every status that may not have received food.
            foreach(CharacterHealthStatus status in characters)
            {
                if(!handledCharacters.Contains(status.CharacterData.name))
                {
                    status.OnEndOfDay(null);
                }
            }
        }
    }

    private CharacterHealthStatus GetHealthStatus(string name)
    {
        return characters.Find(status => status.CharacterData.name == name);
    }
}
