using Articy.TheMigrantsChronicles;
using Articy.TheMigrantsChronicles.GlobalVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Stores conditions that are used in various systems.
 * Conditions are basically only a string, and you can set a condition (add it to a list) and check whether that condition exists (is in the list).
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
 * After that (for int and float keys) you can add a comparison. The same comparisons are supported as the health conditions.  
 * Examples of a game condition:
 *      * 'game:daysincity>2': Checks if the player has spent more than 2 days in the current city / on the ship.
 *      
 * Warning: It is expected that you don't have errors in the condition and follow exactly the rules, i.e. no whitespaces, nothing other than expected.
 */
public class DialogConditionProvider
{
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
            // Only handle bool conditions
            if(ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(condition))
            {
                if(ArticyGlobalVariables.Default.GetVariableByString<bool>(condition))
                {
                    // If  the variable is set by default, add it  to the global conditions.
                    globalConditions.Add(condition);
                }

                // Add a callback that gets called whenever the value changes.
                ArticyGlobalVariables.Default.Notifications.AddListener(condition, OnArticyVariableChanged);
            }
        }
    }

    private void OnArticyVariableChanged(string condition, object value)
    {
        if((bool) value)
        {
            // If the new value is true, add it to our conditions.
            globalConditions.Add(condition);
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

        if(ArticyGlobalVariables.VariableNames.Contains(condition))
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
        LessEqual
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
                        case Operation.None: operation = Operation.Equals; break;
                    }
                    break;

                case '!':
                    key = condition[..i];
                    operation = Operation.NotEquals;
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
}
