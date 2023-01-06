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
    private ProtagonistHealthData healthData;
    private int requiredFoodAmount = 0;
    
    public int DaysWithoutEnoughFood { get; private set; }

    public HealthStatus_Hungry(ProtagonistHealthData data)
    {
        healthData = data;
    }

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
            // Notify the health data to increase homesickness
            healthData.OnDayWithoutEnoughFood(DaysWithoutEnoughFood);
        }
    }
}

public class HealthStatus_Homesickness
{
    private float value = 1.0f;
    private int daysSinceLastDecrease = 0;
    
    public float Value { get { return value; } }
    public int ValueInt { get { return Mathf.FloorToInt(value); } }

    public void OnEndOfDay()
    {
        if(++daysSinceLastDecrease >= 5)
        {
            // Every 5 days, homesickness is decreased by 1.
            AddValue(-1);
            daysSinceLastDecrease = 0;
        }
    }

    public void AddValue(float change)
    {
        Debug.Log($"Homesickness: {value} + {change} = {Mathf.Clamp(value + change, 1, 10)}");
        value = Mathf.Clamp(value + change, 1, 10);
    }
}

public class ProtagonistHealthData
{
    private HealthStatus healthStatus;
    private HealthStatus_Hungry hungryStatus;
    private HealthStatus_Homesickness homesicknessStatus = new HealthStatus_Homesickness();

    public ProtagonistData CharacterData { get; private set; }
    public HealthStatus_Hungry HungryStatus { get { return hungryStatus; } }
    public HealthStatus_Homesickness HomesickessStatus { get { return homesicknessStatus; } }

    public ProtagonistHealthData(HealthStatus status)
    {
        healthStatus = status;
        hungryStatus = new HealthStatus_Hungry(this);
    }

    public void Init(ProtagonistData characterData)
    {
        CharacterData = characterData;
    }

    public void OnEndOfDay(EndOfDayHealthData healthData)
    {
        hungryStatus.OnEndOfDay(healthData != null ? healthData.foodAmount : 0);
        homesicknessStatus.OnEndOfDay();
    }

    public void OnDayWithoutEnoughFood(int daysWithoutEnoughFood)
    {
        if(daysWithoutEnoughFood >= 2)
        {
            // If the character does not have enough food and is hungry, increase the homesickness.
            homesicknessStatus.AddValue(healthStatus.HomesicknessHungryIncrease);
        }
    }
}

public class HealthStatus : MonoBehaviour
{
    [SerializeField, Tooltip("How much homesickness will increase for every day without enough food (starting from the 2. day)")]
    private float homesicknessHungryIncrease = 0.5f;
    [SerializeField, Tooltip("How much homesickness will increase for every item that was stolen")]
    private float homesicknessItemStolenIncrease = 0.3f;
    [SerializeField, Tooltip("How much homesickness will decrease for every item bought")]
    private float homesicknessItemBoughtDecrease = 0.3f;

    private List<ProtagonistHealthData> characters = new List<ProtagonistHealthData>();
    private int dialogsStartedToday = 0;

    public IEnumerable<ProtagonistHealthData> Characters { get { return characters; } }
    public float HomesicknessHungryIncrease { get { return homesicknessHungryIncrease; } }

    private void Start()
    {
        Init(NewGameManager.Instance.PlayableCharacterData.protagonistData);
    }

    public void Init(IEnumerable<ProtagonistData> characterData)
    {
        foreach(ProtagonistData character in characterData)
        {
            ProtagonistHealthData status = new ProtagonistHealthData(this);
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
            ProtagonistHealthData status = GetHealthStatus(healthData.name);
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
            foreach(ProtagonistHealthData status in characters)
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

    private ProtagonistHealthData GetHealthStatus(string name)
    {
        return characters.Find(status => status.CharacterData.name == name);
    }

    public ProtagonistData TryStartDialog()
    {
        ++dialogsStartedToday;

        // Go through the characters and find one that is hungry for 2 days or more.
        ProtagonistHealthData responsibleCharacter = null;
        foreach(ProtagonistHealthData status in characters)
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

    public void OnItemsStolen(int count = 1)
    {
        foreach(ProtagonistHealthData healthData in characters)
        {
            healthData.HomesickessStatus.AddValue(homesicknessItemStolenIncrease * count);
        }
    }

    public void OnItemsBought(int count = 1)
    {
        foreach (ProtagonistHealthData healthData in characters)
        {
            healthData.HomesickessStatus.AddValue(-homesicknessItemBoughtDecrease * count);
        }
    }
}
