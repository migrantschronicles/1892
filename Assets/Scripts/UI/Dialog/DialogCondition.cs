using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ConditionType
{
    And,
    Or
}

[System.Serializable]
public class DialogConditionValue
{
    public string Condition;
    public bool Not;

    public bool Test()
    {
        bool test = NewGameManager.Instance.conditions.HasCondition(Condition);
        return Not ? !test : test;
    }
}

[System.Serializable]
public class DialogCondition
{
    public ConditionType ConditionType;
    public DialogConditionValue[] Children;

    public bool Test()
    {
        switch (ConditionType)
        {
            case ConditionType.And:
            {
                foreach (DialogConditionValue condition in Children)
                {
                    if (!condition.Test())
                    {
                        return false;
                    }
                }

                return true;
            }

            case ConditionType.Or:
            {
                foreach (DialogConditionValue condition in Children)
                {
                    if (condition.Test())
                    {
                        return true;
                    }
                }

                return true;
            }
        }

        return true;
    }

    /**
     * @returns A list of all non-empty conditions that the condition depends on.
     */
    public IEnumerable<string> GetAllConditions()
    {
        return Children.Select(value => value.Condition).Where(condition => !string.IsNullOrWhiteSpace(condition));
    }

    public bool IsEmpty()
    {
        return Children.Length == 0 || !Children.Any(condition => !string.IsNullOrWhiteSpace(condition.Condition));
    }
}

[System.Serializable]
public class SetCondition
{
    public string Condition;
    public bool IsGlobal;
}
