using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        List<string> conditions = GetConditionList(global);
        if(!conditions.Contains(condition))
        {
            conditions.Add(condition);

            if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
            {
                onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
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

        bool successful = false;
        successful |= localConditions.Remove(condition);
        successful |= globalConditions.Remove(condition);
        if(successful)
        {
            if(onConditionsChangedListeners.TryGetValue(condition, out OnConditionsChangedEventData onConditionsChanged))
            {
                onConditionsChanged.onConditionsChanged?.Invoke(onConditionsChanged.context);
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

    /**
     * Checks if one condition is met. Does not care whether it is met globally or locally.
     * If condition is empty, it is considered to be met.
     */
    public bool HasCondition(string condition)
    {
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
