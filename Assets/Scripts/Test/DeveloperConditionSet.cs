using Articy.TheMigrantsChronicles.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperConditionSet : MonoBehaviour
{
    [SerializeField]
    private InputField nameField;
    [SerializeField]
    private InputField valueField;
    [SerializeField]
    private Text outputText;

    public void OnSet()
    {
        string conditionName = nameField.text;
        if(string.IsNullOrEmpty(conditionName))
        {
            outputText.text = "No condition name";
            return;
        }

        string conditionValue = valueField.text;
        if(string.IsNullOrEmpty(conditionValue))
        {
            outputText.text = "No condition value";
            return;
        }

        if(ArticyGlobalVariables.VariableNames.Contains(conditionName))
        {
            if (ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(conditionName))
            {
                bool bValue = conditionValue.Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
                ArticyGlobalVariables.Default.SetVariableByString(conditionName, bValue);
                outputText.text = $"Set condition to '{(bValue ? "true" : "false")}'";
            }
            else if (ArticyGlobalVariables.Default.IsVariableOfTypeInteger(conditionName))
            {
                if(int.TryParse(conditionValue, out int value))
                {
                    ArticyGlobalVariables.Default.SetVariableByString(conditionName, value);
                    outputText.text = $"Set condition to '{value}'";
                }
                else
                {
                    outputText.text = "Could not parse integer value";
                }
            }
            else
            {
                ArticyGlobalVariables.Default.SetVariableByString(conditionName, conditionValue);
                outputText.text = $"Set condition to '{conditionValue}'";
            }
        }
        else
        {
            bool bValue = conditionValue.Equals("true", System.StringComparison.InvariantCultureIgnoreCase);
            if (bValue)
            {
                NewGameManager.Instance.conditions.AddCondition(conditionName, true);
                outputText.text = $"Added condition '{conditionName}'";
            }
            else
            {
                NewGameManager.Instance.conditions.RemoveCondition(conditionName);
                outputText.text = $"Removed condition '{conditionName}'";
            }
        }
    }
}
