using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetConditionsOnClick : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    public SetCondition[] setConditions;
    [SerializeField]
    public string[] unsetConditions;

    private void Awake()
    {
        if(button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void Start()
    {
        if(button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        NewGameManager.Instance.conditions.AddConditions(setConditions);
        NewGameManager.Instance.conditions.RemoveConditions(unsetConditions);
    }
}
