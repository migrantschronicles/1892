using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateManager
{
    public StateModel GetCurrentState();

    public int GetCurrentCityId();

    public void UpdateHealth(int delta);

    #region Space Manipulations

    public void MoveToCity(int cityId);

    #endregion

    #region Resource Manipulations

    public void EndCurrentDay();

    public void SubtractMoney(int amount);

    public void AddMoney(int amount);

    public void SellItem(int itemId, int price);

    public void BuyItem(int itemId, int price);

    public void BuyFood(int portionNumber, int portionPrice);

    #endregion
}
