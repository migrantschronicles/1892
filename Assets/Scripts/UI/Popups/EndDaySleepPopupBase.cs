using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EndDaySleepPopupBase : MonoBehaviour, IPopup
{
    [SerializeField]
    protected EndDayFoodCounter foodCounter;
    [SerializeField]
    protected GameObject endDayPortraitContainer;
    [SerializeField]
    protected GameObject endDayPortraitPrefab;

    protected List<EndDayPortrait> portraits = new();
    protected int inventoryFoodCount = 0;
    protected int distributedFoodCount = 0;

    protected void CreatePortraits()
    {
        foreach(ProtagonistData data in NewGameManager.Instance.PlayableCharacterData.protagonistData)
        {
            GameObject endDayPortraitGO = Instantiate(endDayPortraitPrefab, endDayPortraitContainer.transform);
            EndDayPortrait portrait = endDayPortraitGO.GetComponent<EndDayPortrait>();
            portrait.Init(data);
            portrait.OnPortraitClicked += OnPortraitClicked;
            portraits.Add(portrait);
        }
    }

    protected void InitFoodCounter()
    {
        inventoryFoodCount = NewGameManager.Instance.inventory.GetItemTypeCount(ItemType.Food);
        foodCounter.Count = inventoryFoodCount;
    }

    protected virtual void OnPortraitClicked(EndDayPortrait portrait)
    {
        DistributeFoodTo(portrait);
    }

    protected virtual void DistributeFoodTo(EndDayPortrait portrait)
    {
        if(!CanDistributeFoodTo(portrait))
        {
            return;
        }

        ++portrait.FoodAmount;
        --foodCounter.Count;
        ++distributedFoodCount;
    }

    protected virtual bool CanDistributeFoodTo(EndDayPortrait portrait)
    {
        if(foodCounter.Count <= 0 && !foodCounter.IsInfinite)
        {
            return false;
        }

        ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(portrait.ProtagonistName);
        return portrait.FoodAmount < healthData.HungryStatus.NextRequiredFoodAmount;
    }

    public List<EndOfDayHealthData> GetEndOfDayHealthData()
    {
        return new List<EndOfDayHealthData>(portraits.Select(portrait => new EndOfDayHealthData
        {
            name = portrait.ProtagonistName,
            foodAmount = portrait.FoodAmount
        }));
    }
}
