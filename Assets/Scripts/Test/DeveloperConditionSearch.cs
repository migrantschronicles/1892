using Articy.TheMigrantsChronicles.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperConditionSearch : MonoBehaviour
{
    [SerializeField]
    private InputField nameField;
    [SerializeField]
    private Text output;
    
    public void Search()
    {
        output.text = "";

        string conditionName = nameField.text;
        if(string.IsNullOrWhiteSpace(conditionName))
        {
            return;
        }

        if(ArticyGlobalVariables.VariableNames.Contains(conditionName))
        {
            if(ArticyGlobalVariables.Default.IsVariableOfTypeBoolean(conditionName))
            {
                bool value = ArticyGlobalVariables.Default.GetVariableByString<bool>(conditionName);
                output.text = value ? "true" : "false";
            }
            else if(ArticyGlobalVariables.Default.IsVariableOfTypeInteger(conditionName))
            {
                int value = ArticyGlobalVariables.Default.GetVariableByString<int>(conditionName);
                output.text = value.ToString();
            }
            else
            {
                string value = ArticyGlobalVariables.Default.GetVariableByString<string>(conditionName);
                output.text = value;
            }

            return;
        }

        if(NewGameManager.Instance.conditions.HasCondition(conditionName))
        {
            output.text = "true";
            return;
        }

        output.text = "false";
    }
}
