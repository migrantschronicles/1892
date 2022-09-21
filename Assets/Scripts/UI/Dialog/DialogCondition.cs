using System.Collections;
using System.Collections.Generic;
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
        bool test = DialogSystem.Instance.HasCondition(Condition);
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
}

[System.Serializable]
public class SetCondition
{
    public string Condition;
    public bool IsGlobal;
}
