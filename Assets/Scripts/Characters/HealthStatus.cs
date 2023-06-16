using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * The health status for the characters.
 * There are HealthStatus_[Hungry|Seasickness|Cholera|Homesickness], which represent the data for the sicknesses of one character.
 * ProtagonistHealthData combines all health states and represents the total health state of one character.
 * HealthStatus creates one ProtagonistHealthData for each protagonist and is the health state of all characters.
 */

/**
 * An enum representing the most heavily weighted sickness state of a character.
 */
public enum HealthState
{
    Neutral = 0,
    Angry = 1,
    Happy = 2,
    Hungry = 3,
    Sad = 4,
    Sick = 5
}

/**
 * Data that is passed around at the end of a day for each character.
 */
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
    public int NextRequiredFoodAmount 
    { 
        get 
        {
            // Every day the character needs the double amount plus one for this day.
            int nextRequiredFoodAmount = requiredFoodAmount * 2;
            if(requiredFoodAmount == 0)
            {
                // If the character is not hungry at all, he requires 1 the next time.
                nextRequiredFoodAmount = 1;
            }

            if (healthData.CholeraStatus.IsSick)
            {
                // Sick people need double food amount
                nextRequiredFoodAmount *= 2;
            }

            return nextRequiredFoodAmount;
        } 
    }

    public HealthStatus_Hungry(ProtagonistHealthData data)
    {
        healthData = data;
    }

    public void OnEndOfDay(int receivedFoodAmount)
    {
        requiredFoodAmount = NextRequiredFoodAmount;

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

    public void Save(SaveDataHealthPerCharacter health)
    {
        health.requiredFoodAmount = requiredFoodAmount;
        health.daysWithoutEnoughFood = DaysWithoutEnoughFood;
    }

    public void Load(SaveDataHealthPerCharacter health)
    {
        requiredFoodAmount = health.requiredFoodAmount;
        DaysWithoutEnoughFood = health.daysWithoutEnoughFood;
    }
}

public class HealthStatus_Homesickness
{
    private float value = 1.0f;
    private int daysSinceLastDecrease = 0;
    
    public float Value { get { return value; } }
    public int ValueInt { get { return Mathf.FloorToInt(value); } }
    public int DaysSick { get; private set; }

    public void OnEndOfDay()
    {
        if(++daysSinceLastDecrease >= 5)
        {
            // Every 5 days, homesickness is decreased by 1.
            AddValue(-1);
            daysSinceLastDecrease = 0;
        }

        if(ValueInt > 1)
        {
            ++DaysSick;
        }
        else
        {
            DaysSick = 0;
        }
    }

    public void AddValue(float change)
    {
        value = Mathf.Clamp(value + change, 1, 10);
    }

    public void Save(SaveDataHealthPerCharacter health)
    {
        health.homesickness = value;
        health.homesicknessDaysSinceLastDecrease = daysSinceLastDecrease;
    }

    public void Load(SaveDataHealthPerCharacter health)
    {
        value = health.homesickness;
        daysSinceLastDecrease = health.homesicknessDaysSinceLastDecrease;
    }
}

public class HealthStatus_Cholera
{
    public enum CholeraStatus
    {
        Healthy,
        Exposed,
        Sick
    }

    private static readonly float CHANCE_TO_GET_SICK = 0.1f;
    private static readonly float DAYS_AFTER_EXPOSED_TO_GET_SICK = 2;
    private static readonly float CHANCE_TO_HEAL = 0.1f;

    private CholeraStatus status = CholeraStatus.Healthy;
    private int daysSinceExposed = 0;
    private int daysSick = 0;

    public int DaysSick { get { return daysSick; } }
    public bool IsSick { get { return status == CholeraStatus.Sick; } }
    public bool IsExposed { get { return status == CholeraStatus.Exposed; } }

    public void OnExposed()
    {
        switch(status)
        {
            case CholeraStatus.Healthy:
                // Set the status to exposed.
                daysSinceExposed = 0;
                status = CholeraStatus.Exposed;
                break;
        }

        // Cannot get sick if already exposed / sick.
    }

    public void OnEndOfDay()
    {
        switch(status)
        {
            case CholeraStatus.Exposed:
                if(++daysSinceExposed >= DAYS_AFTER_EXPOSED_TO_GET_SICK)
                {
                    daysSinceExposed = 0;
                    if (UnityEngine.Random.value < CHANCE_TO_GET_SICK)
                    {
                        // The character was exposed and is now actually sick
                        status = CholeraStatus.Sick;
                        daysSick = 1;
                    }
                    else
                    {
                        // The character was exposed but did not get sick
                        status = CholeraStatus.Healthy;
                    }
                }
                break;

            case CholeraStatus.Sick:
                ++daysSick;
                if(UnityEngine.Random.value < CHANCE_TO_HEAL)
                {
                    // The character healed.
                    status = CholeraStatus.Healthy;
                    daysSick = 0;
                }
                break;
        }
    }

    public void Save(SaveDataHealthPerCharacter health)
    {
        health.choleraStatus = status;
        health.choleraDaysSinceExposed = daysSinceExposed;
        health.choleraDaysSick = daysSick;
    }

    public void Load(SaveDataHealthPerCharacter health)
    {
        status = health.choleraStatus;
        daysSinceExposed = health.choleraDaysSinceExposed;
        daysSick = health.choleraDaysSick;
    }
}

public class HealthStatus_Seasickness
{
    private ProtagonistHealthData healthData;
    private bool canGetSeasick = false;
    private bool isOnShip = false;

    public bool IsCurrentlySeasick { get { return canGetSeasick && isOnShip; } }
    public bool CanGetSeasick { get { return canGetSeasick; } }

    public HealthStatus_Seasickness(ProtagonistHealthData healthData)
    {
        this.healthData = healthData;
    }

    public void Init(ProtagonistData data)
    {
        if(UnityEngine.Random.value < data.canGetSeasickProbability)
        {
            canGetSeasick = true;
        }
    }

    public void SetIsOnShip(bool isOnShip)
    {
        this.isOnShip = isOnShip;
    }

    public void OnEndOfDay()
    {
        if(canGetSeasick && isOnShip)
        {
            healthData.OnSeasickDay();
        }
    }

    public void Save(SaveDataHealthPerCharacter health)
    {
        health.canGetSeasick = canGetSeasick;
    }

    public void Load(SaveDataHealthPerCharacter health)
    {
        canGetSeasick = health.canGetSeasick;
    }
}

/**
 * The health state for one character.
 */
public class ProtagonistHealthData
{
    private HealthStatus healthStatus;
    private HealthStatus_Hungry hungryStatus;
    private HealthStatus_Homesickness homesicknessStatus = new();
    private HealthStatus_Cholera choleraStatus = new();
    private HealthStatus_Seasickness seasicknessStatus;
    private HealthState healthState = HealthState.Happy;

    public ProtagonistData CharacterData { get; private set; }
    public HealthStatus_Hungry HungryStatus { get { return hungryStatus; } }
    public HealthStatus_Homesickness HomesickessStatus { get { return homesicknessStatus; } }
    public HealthStatus_Cholera CholeraStatus { get { return choleraStatus; } }
    public HealthStatus_Seasickness SeasicknessStatus { get { return seasicknessStatus; } }
    public HealthState HealthState { get { return healthState; } }

    public delegate void OnHealthChangedEvent(ProtagonistHealthData data);
    public event OnHealthChangedEvent onHealthChanged;

    public delegate void OnHealthStateChangedEvent(ProtagonistHealthData data);
    public event OnHealthStateChangedEvent onHealthStateChanged;

    public ProtagonistHealthData(HealthStatus status)
    {
        healthStatus = status;
        hungryStatus = new HealthStatus_Hungry(this);
        seasicknessStatus = new HealthStatus_Seasickness(this);
    }

    public void Init(ProtagonistData characterData)
    {
        CharacterData = characterData;
        seasicknessStatus.Init(characterData);
    }

    public void OnEndOfDay(EndOfDayHealthData healthData)
    {
        hungryStatus.OnEndOfDay(healthData != null ? healthData.foodAmount : 0);
        homesicknessStatus.OnEndOfDay();
        CholeraStatus.OnEndOfDay();
        seasicknessStatus.OnEndOfDay();
        OnHealthChanged();
    }

    public void OnDayWithoutEnoughFood(int daysWithoutEnoughFood)
    {
        if(daysWithoutEnoughFood >= 2)
        {
            // If the character does not have enough food and is hungry, increase the homesickness.
            // Called from HealthStatus_Hungry::OnEndOfDay, so no need to broadcast because this::OnEndOfDay takes care of it.
            homesicknessStatus.AddValue(healthStatus.HomesicknessHungryIncrease);
        }
    }

    public void OnSeasickDay()
    {
        // Called from HealthStatus_Seasickness::OnEndOfDay, so no need to broadcast
        homesicknessStatus.AddValue(healthStatus.HomesicknessSeasickIncrease);
    }

    public void AddHomesicknessValue(float value)
    {
        HomesickessStatus.AddValue(value);
        OnHealthChanged();
    }

    private void OnHealthChanged()
    {
        HealthState newState = CalculateHealthState();
        if (newState != healthState)
        {
            healthState = newState;
            onHealthStateChanged?.Invoke(this);
        }
        onHealthChanged?.Invoke(this);
    }

    private HealthState CalculateHealthState()
    {
        if (CholeraStatus.IsSick)
        {
            return HealthState.Sick;
        }
        else if (HungryStatus.DaysWithoutEnoughFood >= 2)
        {
            return HealthState.Hungry;
        }
        else if (HomesickessStatus.Value >= 5.0f)
        {
            return HealthState.Sad;
        }
        else if (CholeraStatus.IsExposed || HungryStatus.DaysWithoutEnoughFood > 0 || HomesickessStatus.Value > 2.5f)
        {
            return HealthState.Neutral;
        }
        else
        {
            return HealthState.Happy;
        }
    }

    public void SetIsOnShip(bool value)
    {
        seasicknessStatus.SetIsOnShip(value);
    }

    public SaveDataHealthPerCharacter Save()
    {
        SaveDataHealthPerCharacter health = new SaveDataHealthPerCharacter();
        hungryStatus.Save(health);
        homesicknessStatus.Save(health);
        choleraStatus.Save(health);
        seasicknessStatus.Save(health);
        health.characterName = CharacterData.name;
        return health;
    }

    public void Load(SaveDataHealthPerCharacter health)
    {
        hungryStatus.Load(health);
        homesicknessStatus.Load(health);
        choleraStatus.Load(health);
        seasicknessStatus.Load(health);
        OnHealthChanged();
    }
}

/**
 * The health state of all characters.
 */
public class HealthStatus : MonoBehaviour
{
    [SerializeField, Tooltip("How much homesickness will increase for every day without enough food (starting from the 2. day)")]
    private float homesicknessHungryIncrease = 0.5f;
    [SerializeField, Tooltip("How much homesickness will increase for every item that was stolen")]
    private float homesicknessItemStolenIncrease = 0.3f;
    [SerializeField, Tooltip("How much homesickness will decrease for every item bought")]
    private float homesicknessItemBoughtDecrease = 0.3f;
    [SerializeField, Tooltip("How much homesickness will decrease for every line the other person says")]
    private float homesicknessLeftLineDecrease = 0.025f;
    [SerializeField, Tooltip("How much homesickness will decrease for every line the protagonist says")]
    private float homesicknessRightLineDecrease = 0.025f;
    [SerializeField, Tooltip("How much homesickness will decrease for every decision option taken")]
    private float homesicknessDecisionDecrease = 0.025f;
    [SerializeField, Tooltip("How much homesickness will increase for every day on a ship if the person is seasick")]
    private float homesicknessSeasickIncrease = 1.0f;

    private List<ProtagonistHealthData> characters = new List<ProtagonistHealthData>();
    private int dialogsStartedToday = 0;

    public IEnumerable<ProtagonistHealthData> Characters { get { return characters; } }
    public float HomesicknessHungryIncrease { get { return homesicknessHungryIncrease; } }
    public float HomesicknessSeasickIncrease { get { return homesicknessSeasickIncrease; } }

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

        // Check if any character has cholera for more than 5 days (=> dead)
        foreach(ProtagonistHealthData status in characters)
        {
            if(status.CholeraStatus.DaysSick >= 5)
            {
                // A protagonist died from cholera.
                NewGameManager.Instance.OnProtagonistDied(status.CharacterData);
                // Does not matter if more than one protagonist died.
                break;
            }
        }
    }

    public ProtagonistHealthData GetHealthStatus(string name)
    {
        return characters.Find(status => status.CharacterData.name == name);
    }

    public ProtagonistHealthData GetMainHealthStatus()
    {
        return characters.Find(status => status.CharacterData.isMainProtagonist);
    }

    /**
     * Called when the player tries to start a dialog.
     * If you are able to start a dialog, this returns null.
     * If you can't start a dialog, this returns the reason.
     */
    public ProtagonistHealthData TryStartDialog(bool canStartEvenIfSick, bool canStartEvenIfHungry)
    {
        // Check first if the main protagonist is sick
        ProtagonistHealthData mainProtagonist = GetMainHealthStatus();
        if(mainProtagonist.CholeraStatus.IsSick && !canStartEvenIfSick)
        {
            // The main protagonist is sick and the dialog can't start even if sick.
            return mainProtagonist;
        }

        if(mainProtagonist.HomesickessStatus.Value > 5)
        {
            // The main protagonist is homesick and can only talk to one person.
            if(dialogsStartedToday >= 1)
            {
                return mainProtagonist;
            }
        }

        // The main protagonist is health or the dialog is for a family member (can start even if sick).
        ++dialogsStartedToday;

        // Go through the characters and find one that is hungry for 2 days or more.
        ProtagonistHealthData responsibleCharacter = null;
        foreach(ProtagonistHealthData status in characters)
        {
            // Check if the characters didn't have food for 2 days.
            if(status.HungryStatus.DaysWithoutEnoughFood >= 2 && !canStartEvenIfHungry)
            {
                // If the main character is hungry for more than 2 days, we want to display him as the reason, even if a child is hungry too.
                if(responsibleCharacter == null || !responsibleCharacter.CharacterData.isMainProtagonist)
                {
                    responsibleCharacter = status;
                }
            }
        }

        // If responsibleCharacter is not null, it means that at least one character is hungry for more than 2 days, so the player can only start 1 dialogs.
        if(responsibleCharacter != null)
        {
            if(dialogsStartedToday > 1)
            {
                return responsibleCharacter;
            }
        }

        // No character is hungry for more than 2 days or the player has started less than 1 dialogs.
        return null;
    }

    private void AddHomesicknessValue(float value)
    {
        foreach (ProtagonistHealthData healthData in characters)
        {
            healthData.AddHomesicknessValue(value);
        }
    }

    public void OnItemsStolen(int count = 1)
    {
        AddHomesicknessValue(homesicknessItemStolenIncrease * count);
    }

    public void OnItemsBought(int count = 1)
    {
        AddHomesicknessValue(-homesicknessItemBoughtDecrease * count);
    }

    public void OnDialogLine(bool isMainProtagonist)
    {
        AddHomesicknessValue(!isMainProtagonist ? -homesicknessLeftLineDecrease : -homesicknessRightLineDecrease);
    }

    public void OnDialogDecision()
    {
        AddHomesicknessValue(-homesicknessDecisionDecrease);
    }

    /**
     * @todo Hook this up
     * @return True if the protagonists can travel, false otherwise.
     */
    public bool CanTravel()
    {
        ProtagonistHealthData protagonist = GetMainHealthStatus();
        if(protagonist.CholeraStatus.IsSick)
        {
            // If the main protagonist is sick, you can't travel.
            return false;
        }

        return true;
    }

    public void SetIsOnShip(bool value)
    {
        foreach(ProtagonistHealthData healthData in characters)
        {
            healthData.SetIsOnShip(value);
        }
    }

    public IEnumerable<ProtagonistHealthData> GetHungryCharacters()
    {
        return characters.Where(character => character.HungryStatus.DaysWithoutEnoughFood > 0);
    }

    public List<SaveDataHealthPerCharacter> CreateSaveData()
    {
        return new List<SaveDataHealthPerCharacter>(characters.Select(character => character.Save()));
    }

    public void LoadFromSaveData(List<SaveDataHealthPerCharacter> saveData)
    {
        saveData.ForEach(health => {
            ProtagonistHealthData character = characters.First(character => character.CharacterData.name == health.characterName);
            if(character != null)
            {
                character.Load(health);
            }
            else
            {
                Debug.LogError($"Could not find character {health.characterName} of savegame");
            }
        });
    }
}
