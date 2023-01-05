using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public HealthStatus_Hungry HungryStatus { get { return hungryStatus; } }

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
    private int dialogsStartedToday = 0;

    public IEnumerable<CharacterHealthStatus> Characters { get { return characters; } }

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

        // Reset the number of dialogs
        dialogsStartedToday = 0;
    }

    private CharacterHealthStatus GetHealthStatus(string name)
    {
        return characters.Find(status => status.CharacterData.name == name);
    }

    public CharacterHealthData TryStartDialog()
    {
        ++dialogsStartedToday;

        // Go through the characters and find one that is hungry for 2 days or more.
        CharacterHealthStatus responsibleCharacter = null;
        foreach(CharacterHealthStatus status in characters)
        {
            // Check if the characters didn't have food for 2 days.
            if(status.HungryStatus.DaysWithoutEnoughFood >= 2)
            {
                // If the main character is hungry for more than 2 days, we want to display him as the reason, even if a child is hungry too.
                if(responsibleCharacter == null || !responsibleCharacter.CharacterData.isMainProtagonist)
                {
                    responsibleCharacter = status;
                }
            }
        }

        // If responsibleCharacter is not null, it means that at least one character is hungry for more than 2 days, so the player can only start 2 dialogs.
        if(responsibleCharacter != null)
        {
            if(dialogsStartedToday > 2)
            {
                return responsibleCharacter.CharacterData;
            }
        }

        // No character is hungry for more than 2 days or the player has started less than 2 dialogs.
        return null;
    }
}
