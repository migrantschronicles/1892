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
    private int requiredFoodAmount = 0;
    
    public int DaysWithoutEnoughFood { get; private set; }

    public void OnEndOfDay(int receivedFoodAmount)
    {
        // Every day the character needs the double amount plus one for this day.
        requiredFoodAmount = requiredFoodAmount * 2 + 1;
        // Subtract the amount of food the character has received today.
        requiredFoodAmount = Mathf.Max(0, requiredFoodAmount - receivedFoodAmount);

        if(requiredFoodAmount == 0)
        {
            // The character has eaten enough for today and also refilled the missing foods that accumulated the last days.
            DaysWithoutEnoughFood = 0;
        }
        else
        {
            // The character has not received enough food for the day or to refeed the missing food amounts from the last days.
            ++DaysWithoutEnoughFood;
        }
    }
}

public class CharacterHealthStatus
{
    private HealthStatus_Hungry hungryStatus = new HealthStatus_Hungry();

    public CharacterHealthData CharacterData { get; private set; }

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
            handledCharacters.Add(healthData.name);
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
