using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class StateBehavior : MonoBehaviour
{
    public Text DateText;
    public Text TimeText;
    public Text AmPmText;
    public Text MoneyText;
    public Text FoodText;

    private float minuteDelta = 0.4f;
    private float time = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        if (StateManager.CurrentState == null)
        {
            return;
        }

        if (MoneyText.GetComponent<Text>().text != StateManager.CurrentState.AvailableMoney.ToString())
        {
            MoneyText.GetComponent<Text>().text = StateManager.CurrentState.AvailableMoney.ToString();
        }

        if (FoodText.GetComponent<Text>().text != StateManager.CurrentState.AvailableFood.ToString())
        {
            FoodText.GetComponent<Text>().text = StateManager.CurrentState.AvailableFood.ToString();
        }

        var currentDate = StateManager.CurrentState.StartDate + StateManager.CurrentState.ElapsedTime;
        var currentDateText = $"{currentDate.Day}st {currentDate.ToString("MMMM", CultureInfo.InvariantCulture)} {currentDate.Year}";

        if (DateText.GetComponent<Text>().text != currentDateText)
        {
            DateText.GetComponent<Text>().text = currentDateText;
        }

        if (StateManager.CurrentState.FreezeTime)
        {
            time = 0;
        }

        if(time > minuteDelta)
        {
            StateManager.CurrentState.ElapsedTime += TimeSpan.FromMinutes(1);
            TimeText.GetComponent<Text>().text = (StateManager.CurrentState.StartDate + StateManager.CurrentState.ElapsedTime).ToString(@"hh\:mm");
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }

        if(AmPmText.GetComponent<Text>().text != (StateManager.CurrentState.StartDate + StateManager.CurrentState.ElapsedTime).ToString("tt"))
        {
            AmPmText.GetComponent<Text>().text = (StateManager.CurrentState.StartDate + StateManager.CurrentState.ElapsedTime).ToString("tt");
        }
    }
}
