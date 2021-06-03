using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : IStateManager
{
    private StateModel currentState;

    public StateManager()
    {
        currentState = new StateModel()
        {
            StartDate = Constants.StartDate,
            AvailableMoney = Constants.InitialBalance,
            AvailableFood = Constants.InitialFood,
            LuggageNumber = Constants.InitialLuggageNumber,
            AvailableCityIds = Constants.InitialCityIds,
            AvailableItemIds = Constants.InitialItemIds
        };
    }

    public StateModel GetCurrentState()
    {
        return currentState;
    }

    public int GetCurrentCityId()
    {
        return currentState.CurrentCityId;
    }

    public void UpdateHealth(int delta)
    {
        currentState.CurrentHealth += delta;
    }

    #region Space Manipulations

    public void MoveToCity(int cityId)
    {
        currentState.PreviousCityId = currentState.CurrentCityId;
        currentState.CurrentCityId = cityId;
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
