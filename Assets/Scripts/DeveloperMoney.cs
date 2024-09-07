using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperMoney : MonoBehaviour
{
    [SerializeField]
    private InputField moneyField;

    public void SetMoney()
    {
        if (int.TryParse(moneyField.text, out int value))
        {
            NewGameManager.Instance.SetMoney(value);
        }
    }
}
