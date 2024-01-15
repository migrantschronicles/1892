using Articy.TheMigrantsChronicles;
using Articy.TheMigrantsChronicles.GlobalVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

/**
 * Stores conditions that are used in various systems.
 * Conditions are basically only a string, and you can set a condition (add it to a list) and check whether that condition exists (is in the list).
 * Conditions may not include the following characters: = + - < > ! :
 * 
 * There are special conditions you can use for retrieving health information. These conditions need to start with 'health:'.
 * Then you can optionally specify for which person you want to retrieve health information. These are currently supported:
 *      * 'main:': The main protagonist (e.g. Elis as the mother of the family)
 *      * 'side:': The side character(s) (e.g. Mattis and Mreis of the family)
 * Then you can specify which key you want to check for. These are currently supported:
 *      * 'dayswithoutenoughfood' [int]: How many days the character(s) did not have enough food in a row
 *      * 'homesickness' [float]: How homesick a character is (1-10, least homesick - most homesick)
 *      * 'exposed' [bool]: If the character was exposed to cholera, but is not sick yet.
 *      * 'sick' [bool]: If the character is sick of cholera.
 *      * 'dayssick' [int]: The number of days that the character is sick.
 * After that (for int and float keys) you can add a comparison. These are currently supported:
 *      * '=': Only makes sense for single protagonist queries
 *      * '!=': Only makes sense for single protagonist queries
 *      * '<'
 *      * '<='
 *      * '>'
 *      * '>='
 * Examples of a health condition:
 *      * 'health:dayswithoutenoughfood>2': Checks if any protagonist did not have enough food for more than 2 days
 *      * 'health:main:dayswithoutenoughfood=2': Checks if the main protagonist did not have enough food for exactly 2 days
 *      * 'health:side:dayswithoutenoughfood<=2': Checks if any of the side characters did not have enough food for less than 2 days
 *      
 * There are also special conditions for game stats. These conditions need to start with 'game:'.
 * Then you can specify which key you want to check for. These are currently supported:
 *      * 'daysincity' [int]: How many consecutive days the player spent in the current city (or the ship).
 *      * 'money' [int]: The amount of money the player has.
 *      * 'daysinscene' [int]: How many consecutive days the player spent in the current scene.
 *      * 'cantravelto' [Location]: Whether you can travel to Location
 * After that (for int and float keys) you can add a comparison. The same comparisons are supported as the health conditions.  
 * Examples of a game condition:
 *      * 'game:daysincity>2': Checks if the player has spent more than 2 days in the current city / on the ship.
 *      
 * If you set conditions via AddCondition, you can also change Articy variables or change game state.
 * You can set Articy variables, you have to follow the following syntax:
 * [Variable name][Operation][Value]
 * The variable name has to exist in Articy.
 * The following operations are currently supported:
 *      * '=': Set the variable to the specified value
 *      * '+=': Increment the variable by the specified value (only for Integers)
 *      * '-=': Decrement the variable by the specified value (only for Integers).
 * Examples of changing Articy variables:
 *      * 'CharacterTraits.Corruption+=1': Increases the corruption variable by one.
 *      * 'CharacterTraits.Corruption-=3': Decreases the corruption variable by 3.
 *      * 'CharacterTraits.Corruption=5': Sets the corruption variable to 5.
 * Health related values like homesickness should not be set in Articy variables directly.
 * Instead change if with 'health:'.
 *      
 * You can also set health related values of the game. These conditions need to start with 'health:'.
 * Then you can optionally specify for which person you want to retrieve health information. These are currently supported:
 *      * 'main:': The main protagonist (e.g. Elis as the mother of the family)
 *      * 'side:': The side character(s) (e.g. Mattis and Mreis of the family)
 *      * 'elis:': Elis Beffort
 *      * 'mattis:': Matti Beffort
 *      * 'mreis:': Mreis Beffort
 * Then you can specify which value you want to modify. These are currently supported:
 *      * 'homesickness' [float]: Change the homesickness value (1-10, least homesick - most homesick)
 * After that you can set the operation and value. The same operations are supported as changing Articy variables.
 *      
 * Warning: It is expected that you don't have errors in the condition and follow exactly the rules, i.e. no whitespaces, nothing other than expected.
 */
public class DialogConditionProvider : MonoBehaviour
{
    [SerializeField]
    private string moneyConditionArticy;
    [SerializeField]
    private string daysInCityConditionArticy;
    [SerializeField]
    private bool verbose = false;

    class OnConditionsChangedEventData
    {
        public OnConditionsChangedEvent onConditionsChanged;
        public object context;
    }

    private List<string> localConditions = new List<string>();
    private List<string> globalConditions = new List<string>();

    public delegate void OnConditionsChangedEvent(object context);
    private readonly Dictionary<string, OnConditionsChangedEventData> onConditionsChangedListeners = new Dictionary<string, OnConditionsChangedEventData>();

    public void Init()
    {
        foreach(string condition in ArticyGlobalVariables.VariableNames)
        {
            if(ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(condition))
            {
                if(ArticyGlobalVariables.Default.GetVariableByString<bool>(condition))
                {
                    // If  the variable is set by default, add it  to the global conditions.
                    globalConditions.Add(condition);
                }
            }

            // Add a callback that gets called whenever the value changes.
            ArticyGlobalVariables.Default.Notifications.AddListener(condition, OnArticyVariableChanged);
        }

        NewGameManager.Instance.onMoneyChanged += OnMoneyChanged;
        OnMoneyChanged(NewGameManager.Instance.money);
        NewGameManager.Instance.onNewDay += OnNewDay;
    }

    private void Start()
    {
        foreach (var status in NewGameManager.Instance.HealthStatus.Characters)
        {
            status.onHealthChanged += OnHealthChanged;
        }
    }

    private void OnMoneyChanged(int money)
    {
        ArticyGlobalVariables.Default.SetVariableByString(moneyConditionArticy, money);
    }

    private void OnNewDay()
    {
        ArticyGlobalVariables.Default.SetVariableByString(daysInCityConditionArticy, NewGameManager.Instance.DaysInCity);
    }

    private string GetArticyPrefix(string protagonistName)
    {
        switch(protagonistName)
        {
            case "Mattis": return "Metti";
            case "Mreis": return "Mrei";
        }

        return protagonistName;
    }

    private string GetUnityCharacter(string articyName)
    {
        switch(articyName)
        {
            case "Metti": return "Mattis";
            case "Mrei": return "Mreis";
        }

        return articyName;
    }

    private void OnHealthChanged(ProtagonistHealthData data)
    {
        string articyPrefix = GetArticyPrefix(data.CharacterData.name);
        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyPrefix}DaysHungry", data.HungryStatus.DaysWithoutEnoughFood);
        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyPrefix}DaysSick", data.CholeraStatus.DaysSick);
        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyPrefix}Exposed", data.CholeraStatus.IsExposed);
        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyPrefix}Homesickness", data.HomesickessStatus.ValueInt);
        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyPrefix}Sick", data.CholeraStatus.IsSick);
    }

    public void InitHistoryMode(bool isHistoryMode)
    {
        ArticyGlobalVariables.Default.SetVariableByString("Misc.HistoryMode", isHistoryMode);
    }

    private void OnArticyVariableChanged(string condition, object value)
    {
        if(verbose)
        {
            Debug.Log($"On Articy Condition changed: {condition} ({value})");
        }

        if(ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(condition))
        {
            if ((bool)value)
            {
                // If the new value is true, add it to our conditions.
                globalConditions.Add(condition);

                HistoryModeRoute historyModeRoute = HistoryModeRoute.None;
                switch (condition)
                {
                    case "Misc.HistoryModeLeHavre": historyModeRoute = HistoryModeRoute.LeHavre; break;
                    case "Misc.HistoryModeRotterdam": historyModeRoute = HistoryModeRoute.Rotterdam; break;
                    case "Misc.HistoryModeAntwerp": historyModeRoute = HistoryModeRoute.Antwerp; break;
                }

                if (historyModeRoute != HistoryModeRoute.None)
                {
                    NewGameManager.Instance.OnHistoryModeRouteSelected(historyModeRoute);
                }
            }
            else
            {
                // If the new value is false, remove it from our conditions.
                globalConditions.Remove(condition);
            }

            if (onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
            {
                // Call the delegate.
                onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
            }
        }
        else if(condition.StartsWith("Health.") && condition.EndsWith("Homesickness"))
        {
            string articyName = condition.Replace("Health.", "").Replace("Homesickness", "");
            string unityName = GetUnityCharacter(articyName);
            ProtagonistHealthData data = NewGameManager.Instance.HealthStatus.GetHealthStatus(unityName);
            if (data != null)
            {
                // Only do something if the value (unclamped) differs
                if ((int)value != data.HomesickessStatus.ValueInt)
                {
                    // If the homesickness value is not changed (if value is e.g. 0, and the clamped value does not differ)
                    if(!NewGameManager.Instance.HealthStatus.SetHomesicknessValue(unityName, (int)value))
                    {
                        // Set the value in Articy, so Articy does not use wrong values.
                        ArticyGlobalVariables.Default.SetVariableByString($"Health.{articyName}Homesickness", data.HomesickessStatus.ValueInt);
                    }
                }
            }
        }
    }

    /**
     * Adds a listener for when the specified condition changes.
     * @param conditions The conditions to add the listener to.
     * @param onConditionsChanged The delegate to call if the condition changes. Signature: void (object context).
     * @param context Optional user data to pass to the delegate.
     */
    public void AddOnConditionsChanged(IEnumerable<string> conditions, OnConditionsChangedEvent onConditionsChanged, object context = null)
    {
        foreach (string condition in conditions)
        {
            AddOnConditionChanged(condition, onConditionsChanged, context);
        }
    }

    /**
     * Adds a listener for when the specified condition changes.
     * @param condition The condition to listen for.
     * @param onConditionsChanged The delegate to call if the condition changes. Signature: void (object context).
     * @param context Optional user data to pass to the delegate.
     */
    public void AddOnConditionChanged(string condition, OnConditionsChangedEvent onConditionsChanged, object context = null)
    {
        if (onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData changedEvent))
        {
            onConditionsChangedListeners[condition].onConditionsChanged += onConditionsChanged;
        }
        else
        {
            onConditionsChangedListeners.Add(condition, new OnConditionsChangedEventData { onConditionsChanged = onConditionsChanged, context = context });
        }
    }

    public void RemoveOnConditionChanged(string condition, OnConditionsChangedEvent onConditionsChanged)
    {
        if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData changedEvent))
        {
            changedEvent.onConditionsChanged -= onConditionsChanged;
        }
    }

    public void RemoveOnConditionsChanged(IEnumerable<string> conditions, OnConditionsChangedEvent onConditionsChanged)
    {
        foreach(string condition in conditions)
        {
            RemoveOnConditionChanged(condition, onConditionsChanged);
        }
    }

    private void AddHealthCondition(string condition)
    {
        IEnumerable<ProtagonistHealthData> affectedCharacters = NewGameManager.Instance.HealthStatus.Characters;

        // Filter by character
        if (condition.StartsWith("main:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(5);
            affectedCharacters = affectedCharacters.Where(status => status.CharacterData.isMainProtagonist);
        }
        else if (condition.StartsWith("side:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(5);
            affectedCharacters = affectedCharacters.Where(status => !status.CharacterData.isMainProtagonist);
        }
        else if(condition.StartsWith("elis:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(5);
            affectedCharacters = affectedCharacters.Where(status => status.CharacterData.name == "Elis");
        }
        else if (condition.StartsWith("mattis:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(7);
            affectedCharacters = affectedCharacters.Where(status => status.CharacterData.name == "Mattis");
        }
        else if (condition.StartsWith("mreis:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(6);
            affectedCharacters = affectedCharacters.Where(status => status.CharacterData.name == "Mreis");
        }

        SplitOperation(condition, out string key, out Operation operation, out string value);

        if (key.Equals("homesickness", StringComparison.OrdinalIgnoreCase))
        {
            float.TryParse(value, out float valueFloat);
            foreach(ProtagonistHealthData character in affectedCharacters)
            {
                switch(operation)
                {
                    case Operation.Equals: character.SetHomesicknessValue(valueFloat); break;
                    case Operation.IncrementBy: character.AddHomesicknessValue(valueFloat); break;
                    case Operation.DecrementBy: character.AddHomesicknessValue(-valueFloat); break;
                }
            }
        }
    }

    /**
     * Adds a condition to the list.
     * @param global True if it should be added to the global list, false for the local one.
     */
    public void AddCondition(string condition, bool global)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return;
        }

        if(verbose)
        {
            Debug.Log($"Add condition {condition} {global}");
        }

        // Check if this is a special condition
        if(condition.StartsWith("health:"))
        {
            AddHealthCondition(condition[7..]);
            return;
        }

        // Check if this changes a value, or if it is a boolean condition
        SplitOperation(condition, out string key, out Operation operation, out string value);
        if (operation != Operation.None)
        {
            // This sets a condition in Articy.
            if(ArticyGlobalVariables.VariableNames.Contains(key))
            {
                if(int.TryParse(value, out int valueInt))
                {
                    // Calculate the new integer value.
                    int currentValue = ArticyGlobalVariables.Default.GetVariableByString<int>(key);
                    int newValue = currentValue;
                    switch(operation)
                    {
                        case Operation.Equals: newValue = valueInt; break;
                        case Operation.IncrementBy: newValue += valueInt; break;
                        case Operation.DecrementBy: newValue -= valueInt; break;
                    }

                    // Set the new value.
                    ArticyGlobalVariables.Default.SetVariableByString(key, newValue);
                }
                else
                {
                    // Set the string directly.
                    ArticyGlobalVariables.Default.SetVariableByString(key, value);
                }
            }
        }
        else
        {
            // This is a boolean condition, so only set it
            if (ArticyGlobalVariables.VariableNames.Contains(condition))
            {
                // Add the condition to articy
                // The callback for when an articy condition changed handles invoking delegate etc.
                ArticyGlobalVariables.Default.SetVariableByString(condition, true);
            }
            else
            {
                // This is a condition which only exists in our system, not in articy.
                List<string> conditions = GetConditionList(global);
                if (!conditions.Contains(condition))
                {
                    conditions.Add(condition);

                    if (onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
                    {
                        onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
                    }
                }
            }
        }
    }

    /**
     * Adds a condition to the list.
     */
    public void AddCondition(SetCondition condition)
    {
        AddCondition(condition.Condition, condition.IsGlobal);
    }

    /**
     * Adds multiple conditions to the list.
     */
    public void AddConditions(IEnumerable<SetCondition> conditions)
    {
        foreach (SetCondition condition in conditions)
        {
            AddCondition(condition);
        }
    }

    /**
     * Adds multiple conditions to the list.
     */
    public void AddConditions(IEnumerable<string> conditions, bool global)
    {
        foreach (string condition in conditions)
        {
            AddCondition(condition, global);
        }
    }

    /**
     * Removes a condition from the local and global list.
     */
    public void RemoveCondition(string condition)
    {
        if(string.IsNullOrWhiteSpace(condition))
        {
            return;
        }

        if(verbose)
        {
            Debug.Log($"Remove condition {condition}");
        }

        // Set the condition to false in articy
        if(ArticyGlobalVariables.VariableNames.Contains(condition))
        {
            // Set the value to false in articy.
            // The callback handles calling the delegate and removing it from our conditions.
            ArticyGlobalVariables.Default.SetVariableByString(condition, false);
        }
        else
        {
            // This condition does not exist in articy.
            bool successful = false;
            successful |= localConditions.Remove(condition);
            successful |= globalConditions.Remove(condition);
            if (successful)
            {
                if (onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
                {
                    onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
                }
            }
        }
    }

    /**
     * Removes conditions from the local and global list.
     */
    public void RemoveConditions(IEnumerable<string> conditions)
    {
        foreach (string condition in conditions)
        {
            RemoveCondition(condition);
        }
    }

    enum Operation
    {
        None,
        Equals,
        NotEquals,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        IncrementBy,
        DecrementBy
    }

    private void SplitOperation(string condition, out string key, out Operation operation, out string value)
    {
        key = condition;
        operation = Operation.None;
        value = "";
        for(int i = 0; i < condition.Length; i++)
        {
            switch(condition[i])
            {
                case '<':
                    key = condition[..i];
                    operation = Operation.Less;
                    break;

                case '>':
                    key = condition[..i];
                    operation = Operation.Greater;
                    break;

                case '=':
                    switch(operation)
                    {
                        case Operation.Greater: operation = Operation.GreaterEqual; break;
                        case Operation.Less: operation = Operation.LessEqual; break;
                        case Operation.None:
                            key = condition[..i];
                            operation = Operation.Equals; 
                            break;
                    }
                    break;

                case '!':
                    key = condition[..i];
                    operation = Operation.NotEquals;
                    break;

                case '+':
                    key = condition[..i];
                    operation = Operation.IncrementBy;
                    break;

                case '-':
                    key = condition[..i];
                    operation = Operation.DecrementBy;
                    break;

                default:
                    if(operation != Operation.None)
                    {
                        value = condition[i..];
                        return;
                    }
                    break;
            }
        }
    }

    private bool Compare(int key, Operation operation, int value)
    {
        switch(operation)
        {
            case Operation.Equals: return key == value;
            case Operation.NotEquals: return key != value;
            case Operation.Greater: return key > value;
            case Operation.Less: return key < value;
            case Operation.GreaterEqual: return key >= value;
            case Operation.LessEqual: return key <= value;
        }

        return false;
    }

    private bool Compare(float key, Operation operation, float value)
    {
        switch (operation)
        {
            case Operation.Equals: return key == value;
            case Operation.NotEquals: return key != value;
            case Operation.Greater: return key > value;
            case Operation.Less: return key < value;
            case Operation.GreaterEqual: return key >= value;
            case Operation.LessEqual: return key <= value;
        }

        return false;
    }

    private bool HasHealthCondition(string condition)
    {
        IEnumerable<ProtagonistHealthData> affectedCharacters = NewGameManager.Instance.HealthStatus.Characters;

        // Filter by character
        if(condition.StartsWith("main:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(5);
            affectedCharacters = affectedCharacters.Where(status => status.CharacterData.isMainProtagonist);
        }
        else if(condition.StartsWith("side:", StringComparison.OrdinalIgnoreCase))
        {
            condition = condition.Substring(5);
            affectedCharacters = affectedCharacters.Where(status => !status.CharacterData.isMainProtagonist);
        }

        SplitOperation(condition, out string key, out Operation operation, out string value);

        if(key.Equals("dayswithoutenoughfood", StringComparison.OrdinalIgnoreCase))
        {
            foreach(ProtagonistHealthData status in affectedCharacters)
            {
                if (Compare(status.HungryStatus.DaysWithoutEnoughFood, operation, int.Parse(value)))
                {
                    return true;
                }
            }
        }
        else if(key.Equals("homesickness", StringComparison.OrdinalIgnoreCase))
        {
            foreach(ProtagonistHealthData status in affectedCharacters)
            {
                if(Compare(status.HomesickessStatus.Value, operation, float.Parse(value)))
                {
                    return true;
                }
            }
        }
        else if(key.Equals("exposed", StringComparison.OrdinalIgnoreCase))
        {
            foreach(ProtagonistHealthData status in affectedCharacters)
            {
                if(status.CholeraStatus.IsExposed)
                {
                    return true;
                }
            }
        }
        else if(key.Equals("sick", StringComparison.OrdinalIgnoreCase))
        {
            foreach(ProtagonistHealthData status in affectedCharacters)
            {
                if(status.CholeraStatus.IsSick)
                {
                    return true;
                }
            }
        }
        else if(key.Equals("dayssick", StringComparison.OrdinalIgnoreCase))
        {
            foreach(ProtagonistHealthData status in affectedCharacters)
            {
                if(Compare(status.CholeraStatus.DaysSick, operation, int.Parse(value)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool HasGameCondition(string condition)
    {
        SplitOperation(condition, out string key, out Operation operation, out string value);

        if(key.Equals("daysincity", StringComparison.OrdinalIgnoreCase))
        {
            return Compare(NewGameManager.Instance.DaysInCity, operation, int.Parse(value));
        }
        else if(key.Equals("money", StringComparison.OrdinalIgnoreCase))
        {
            return Compare(NewGameManager.Instance.money, operation, int.Parse(value));
        }
        else if(key.Equals("daysinscene", StringComparison.OrdinalIgnoreCase))
        {
            return Compare(LevelInstance.Instance.CurrentScene ? LevelInstance.Instance.CurrentScene.DaysInScene : 0, operation, int.Parse(value));
        }
        else if(key.Equals("cantravelto", StringComparison.OrdinalIgnoreCase))
        {
            return NewGameManager.Instance.CanTravelTo(value);
        }

        return false;
    }

    /**
     * Checks if one condition is met. Does not care whether it is met globally or locally.
     * If condition is empty, it is considered to be met.
     */
    public bool HasCondition(string condition)
    {
        if(condition.StartsWith("health:"))
        {
            return HasHealthCondition(condition[7..]);
        }
        else if(condition.StartsWith("game:"))
        {
            return HasGameCondition(condition[5..]);
        }

        if (string.IsNullOrWhiteSpace(condition))
        {
            return true;
        }

        if (localConditions.Contains(condition))
        {
            return true;
        }

        return globalConditions.Contains(condition);
    }

    private List<string> GetConditionList(bool global)
    {
        return global ? globalConditions : localConditions;
    }

    public void ResetLocalConditions()
    {
        List<string> oldLocalConditions = localConditions;
        localConditions = new List<string>();
        foreach(string condition in oldLocalConditions)
        {
            if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
            {
                onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
            }
        }
    }

    public List<SaveDataCondition> CreateSaveData()
    {
        List<SaveDataCondition> saveConditions = new();

        foreach(string condition in ArticyGlobalVariables.VariableNames)
        {
            if(ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(condition))
            {
                bool value = ArticyGlobalVariables.Default.GetVariableByString<bool>(condition);
                saveConditions.Add(new SaveDataCondition { name = condition, type = SaveDataCondition.Type.Bool, valueBool = value, articy = true });
            }
            else if(ArticyGlobalVariables.Default.IsVariableOfTypeInteger(condition))
            {
                int value = ArticyGlobalVariables.Default.GetVariableByString<int>(condition);
                saveConditions.Add(new SaveDataCondition { name = condition, type = SaveDataCondition.Type.Int, valueInt = value, articy = true });
            }
            else if(ArticyGlobalVariables.Default.IsVariableOfTypeString(condition))
            {
                string value = ArticyGlobalVariables.Default.GetVariableByString<string>(condition);
                saveConditions.Add(new SaveDataCondition { name = condition, type = SaveDataCondition.Type.String, valueString = value, articy = true });
            }
        }

        foreach(string condition in globalConditions)
        {
            if(!ArticyGlobalVariables.VariableNames.Contains(condition))
            {
                saveConditions.Add(new SaveDataCondition { name = condition, type = SaveDataCondition.Type.Bool, valueBool = true, articy = false });
            }
        }

        return saveConditions;
    }

    public void LoadFromSaveData(List<SaveDataCondition> saveConditions)
    {
        foreach(SaveDataCondition condition in saveConditions)
        {
            if(condition.articy)
            {
                switch(condition.type)
                {
                    case SaveDataCondition.Type.Bool:
                        ArticyGlobalVariables.Default.SetVariableByString(condition.name, condition.valueBool);
                        break;

                    case SaveDataCondition.Type.Int:
                        ArticyGlobalVariables.Default.SetVariableByString(condition.name, condition.valueInt);
                        break;

                    case SaveDataCondition.Type.String:
                        ArticyGlobalVariables.Default.SetVariableByString(condition.name, condition.valueString);
                        break;
                }
            }
            else
            {
                globalConditions.Add(condition.name);

                if (onConditionsChangedListeners.TryGetValue(condition.name, out OnConditionsChangedEventData onConditionsChanged))
                {
                    onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
                }
            }
        }
    }
}
