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
    public ConditionType ConditionTypeResult;
    public ConditionType ConditionType2;
    public DialogConditionValue[] Children2;

    private bool Test(IEnumerable<DialogConditionValue> values, ConditionType type)
    {
        switch (type)
        {
            case ConditionType.And:
            {
                if(values != null)
                {
                    foreach (DialogConditionValue condition in values)
                    {
                        if (!condition.Test())
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            case ConditionType.Or:
            {
                int length = 0;
                if(values != null)
                {
                    foreach (DialogConditionValue condition in values)
                    {
                        if (condition.Test())
                        {
                            return true;
                        }

                        ++length;
                    }
                }

                return length == 0;
            }
        }

        return true;
    }

    public bool Test()
    {
        switch (ConditionTypeResult)
        {
            case ConditionType.And:
            {
                if(!Test(Children, ConditionType) || !Test(Children2, ConditionType2))
                {
                    return false;
                }

                return true;
            }

            case ConditionType.Or:
            {
                if(Test(Children, ConditionType) || Test(Children2, ConditionType2))
                {
                    return true;
                }

                return Children.Length == 0 || Children2.Length == 0;
            }
        }

        return true;
    }

    /**
     * @returns A list of all non-empty conditions that the condition depends on.
     */
    public IEnumerable<string> GetAllConditions()
    {
        return Children.Concat((Children2 != null && Children2.Length > 0) ? Children2 : new DialogConditionValue[] { })
            .Select(value => value.Condition).Where(condition => !string.IsNullOrWhiteSpace(condition));
    }

    public bool IsEmpty()
    {
        return Children.Length == 0 || Children2.Length == 0 ||!(Children.Concat(Children2).Any(condition => !string.IsNullOrWhiteSpace(condition.Condition)));
    }
}

[System.Serializable]
public class SetCondition
{
    public string Condition;
    public bool IsGlobal;
}
