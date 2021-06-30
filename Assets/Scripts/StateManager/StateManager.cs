using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : IStateManager
{
    public static State CurrentState;

    public StateManager()
    {
        CurrentState = new State()
        {
            StartDate = new DateTime(1892, 8, 21, 8, 0, 0),
            AvailableMoney = Constants.InitialBalance,
            AvailableFood = Constants.InitialFood,
            LuggageNumber = Constants.InitialLuggageNumber,
            AvailableItemIds = Constants.InitialItemIds,
            CurrentCityName = CityData.Luxembourg,
            AvailableCityNames = new List<string>() { CityData.Luxembourg },
            VisitedCityNames = new List<string>() { CityData.Luxembourg }
        };
    }

    public State GetCurrentState()
    {
        return CurrentState;
    }

    public int GetCurrentCityId()
    {
        return 0;// currentState.CurrentCityName;
    }

    public void UpdateHealth(int delta)
    {
        CurrentState.CurrentHealth += delta;
    }

    #region Space Manipulations

    public void MoveToCity(int cityId)
    {
        CurrentState.PreviousCityName = CurrentState.CurrentCityName;
        CurrentState.CurrentCityName = "";// cityId;
    }

    #endregion

    //Part of the second sprint
    #region Resource Manipulations

    public void EndCurrentDay()
    {
        throw new NotImplementedException();
    }

    public void AddMoney(int amount)
    {
        throw new NotImplementedException();
    }

    public void BuyFood(int portionNumber, int portionPrice)
    {
        throw new NotImplementedException();
    }

    public void BuyItem(int itemId, int price)
    {
        throw new NotImplementedException();
    }

    public void SellItem(int itemId, int price)
    {
        throw new NotImplementedException();
    }

    public void SubtractMoney(int amount)
    {
        throw new NotImplementedException();
    }

    #endregion
}
