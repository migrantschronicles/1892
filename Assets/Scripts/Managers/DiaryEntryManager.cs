using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class DiaryEntryManager : MonoBehaviour
{
    [SerializeField]
    private DiaryEntry luxembourgEntry;
    [SerializeField]
    private DiaryEntry parisEntry;
    [SerializeField]
    private DiaryEntry leHavreEntry;
    [SerializeField]
    private DiaryEntry rotterdamEntry;
    [SerializeField]
    private DiaryEntry bremerhavenEntry;
    [SerializeField]
    private DiaryEntry brusselsEntry;
    [SerializeField]
    private DiaryEntry marseilleEntry;
    [SerializeField]
    private DiaryEntry hamburgEntry;
    [SerializeField]
    private DiaryEntry antwerpEntry;
    [SerializeField]
    private DiaryEntry liverpoolEntry;
    [SerializeField]
    private DiaryEntry southamptonEntry;
    [SerializeField]
    private DiaryEntry genoaEntry;
    [SerializeField]
    private DiaryEntry shipDay1Entry;
    [SerializeField]
    private DiaryEntry shipDay2Entry;
    [SerializeField]
    private DiaryEntry shipDay3Entry;
    [SerializeField]
    private DiaryEntry shipDay4Entry;
    [SerializeField]
    private DiaryEntry shipDay5Entry;
    [SerializeField]
    private DiaryEntry shipDay6Entry;
    [SerializeField]
    private DiaryEntry shipDay7Entry;
    [SerializeField]
    private DiaryEntry shipDay8Entry;
    [SerializeField]
    private DiaryEntry shipDay9Entry;
    [SerializeField]
    private DiaryEntry shipDay10Entry;
    [SerializeField]
    private DiaryEntry minneapolisEntry;
    [SerializeField]
    private DiaryEntry villageOfBelgiumEntry;
    [SerializeField]
    private DiaryEntry newYorkCityEntry;
    [SerializeField]
    private DiaryEntry chicagoEntry;
    [SerializeField]
    private DiaryEntry elisIslandEntry;
    [SerializeField]
    private DiaryEntry bostonEntry;
    [SerializeField]
    private DiaryEntry philadelphiaEntry;
    [SerializeField]
    private DiaryEntry dubuqueEntry;
    [SerializeField]
    private DiaryEntry rollingstoneEntry;
    [SerializeField]
    private DiaryEntry stDonatusEntry;
    [SerializeField]
    private DiaryEntry milwaukeeEntry;

    [SerializeField]
    private LocalizedString and;
    [SerializeField]
    private LocalizedString i;
    [SerializeField]
    private LocalizedString[] transportationFirstDay;
    [SerializeField]
    private LocalizedString transportationOtherDays;
    [SerializeField]
    private LocalizedString lastNightHotelEnoughFood;
    [SerializeField]
    private LocalizedString lastNightHotelNotEnoughFood;
    [SerializeField]
    private LocalizedString lastNightOutsideEnoughFoodNothingStolen;
    [SerializeField]
    private LocalizedString lastNightOutsideNotEnoughFoodNothingStolen;
    [SerializeField]
    private LocalizedString lastNightOutsideEnoughFoodStolen;
    [SerializeField]
    private LocalizedString lastNightOutsideNotEnoughFoodStolen;
    [SerializeField]
    private LocalizedString[] lastNightShip;
    [SerializeField]
    private LocalizedString lastNightShipStopoverDay;
    [SerializeField]
    private LocalizedString healthNoProblems;
    [SerializeField]
    private LocalizedString healthNewProblem;
    [SerializeField]
    private LocalizedString healthNewProblems;
    [SerializeField]
    private LocalizedString healthExistingProblem;
    [SerializeField]
    private LocalizedString luxembourg;
    [SerializeField]
    private LocalizedString paris;
    [SerializeField]
    private LocalizedString brussels;
    [SerializeField]
    private LocalizedString lehavre;
    [SerializeField]
    private LocalizedString rotterdam;
    [SerializeField]
    private LocalizedString bremerhaven;
    [SerializeField]
    private LocalizedString antwerp;
    [SerializeField]
    private LocalizedString marseille;
    [SerializeField]
    private LocalizedString hamburg;
    [SerializeField]
    private LocalizedString liverpool;
    [SerializeField]
    private LocalizedString southampton;
    [SerializeField]
    private LocalizedString genoa;
    [SerializeField]
    private LocalizedString shipDay1;
    [SerializeField]
    private LocalizedString shipDay2;
    [SerializeField]
    private LocalizedString shipDay3;
    [SerializeField]
    private LocalizedString shipDay4;
    [SerializeField]
    private LocalizedString shipDay5;
    [SerializeField]
    private LocalizedString shipDay6;
    [SerializeField]
    private LocalizedString shipDay7;
    [SerializeField]
    private LocalizedString shipDay8;
    [SerializeField]
    private LocalizedString shipDay9;
    [SerializeField]
    private LocalizedString shipDay10;
    [SerializeField]
    private LocalizedString elisIsland;
    [SerializeField]
    private LocalizedString newYorkCity;
    [SerializeField]
    private LocalizedString chicago;
    [SerializeField]
    private LocalizedString boston;
    [SerializeField]
    private LocalizedString milwaukee;
    [SerializeField]
    private LocalizedString villageOfBelgium;
    [SerializeField]
    private LocalizedString minneapolis;
    [SerializeField]
    private LocalizedString rollingstone;
    [SerializeField]
    private LocalizedString dubuque;
    [SerializeField]
    private LocalizedString stDonatus;
    [SerializeField]
    private LocalizedString philadelphia;

    private string GenerateTransportationInfo()
    {
        int daysInCity = NewGameManager.Instance.DaysInCity;
        return GenerateTransportationInfo(daysInCity);
    }

    private string GenerateTransportationInfo(int daysInCity)
    {
        string localizedLocation = NewGameManager.Instance.LocationManager.GetLocalizedName(LevelInstance.Instance.LocationName);
        if (NewGameManager.Instance.DaysInCity == 0)
        {
            // The day of the arrival
            int randomIndex = Random.Range(0, transportationFirstDay.Length);
            string localizedMethod = NewGameManager.Instance.TransportationManager.GetLocalizedMethod(NewGameManager.Instance.lastMethod);
            return LocalizationManager.Instance.GetLocalizedString(transportationFirstDay[randomIndex], localizedLocation, localizedMethod);
        }
        else
        {
            // Other days
            return LocalizationManager.Instance.GetLocalizedString(transportationOtherDays, localizedLocation);
        }
    }

    private string GenerateLastNight()
    {
        List<ProtagonistHealthData> hungryCharacters = new List<ProtagonistHealthData>(NewGameManager.Instance.HealthStatus.GetHungryCharacters());
        SleepMethod sleepMethod = NewGameManager.Instance.LastSleepMethod;
        List<StolenItemInfo> items = NewGameManager.Instance.LastStolenItems;
        int daysInCity = NewGameManager.Instance.DaysInCity;
        string stopoverLocation = NewGameManager.Instance.ShipManager.IsStopoverDay ? NewGameManager.Instance.ShipManager.StopoverLocation : null;
        return GenerateLastNight(hungryCharacters, sleepMethod, items, daysInCity, stopoverLocation);
    }

    private string GenerateLastNight(List<ProtagonistHealthData> hungryCharacters, SleepMethod lastSleepMethod, List<StolenItemInfo> lastStolenItems,
        int daysInCity, string stopoverLocation)
    {
        switch(lastSleepMethod)
        {
            case SleepMethod.Outside:
            {
                if(hungryCharacters.Count == 0)
                {
                    if(lastStolenItems.Count == 0)
                    {
                        // Nothing got stolen, no one is hungry.
                        return LocalizationManager.Instance.GetLocalizedString(lastNightOutsideEnoughFoodNothingStolen);
                    }
                    else
                    {
                        // Something got stolen, but no one is hungry.
                        string localizedItems = GetLocalizedStolenItems(lastStolenItems);
                        return LocalizationManager.Instance.GetLocalizedString(lastNightOutsideEnoughFoodStolen, localizedItems);
                    }
                }
                else
                {
                    if(lastStolenItems.Count == 0)
                    {
                        // Nothing got stolen, but some one is hungry.
                        return LocalizationManager.Instance.GetLocalizedString(lastNightOutsideNotEnoughFoodNothingStolen);
                    }
                    else
                    {
                        // Something got stolen and someone is hungry.
                        string localizedItems = GetLocalizedStolenItems(lastStolenItems);
                        return LocalizationManager.Instance.GetLocalizedString(lastNightOutsideNotEnoughFoodStolen, localizedItems);
                    }
                }
            }

            case SleepMethod.Hotel:
            {
                if(hungryCharacters.Count == 0)
                {
                    // Everyone had enough food.
                    return LocalizationManager.Instance.GetLocalizedString(lastNightHotelEnoughFood);
                }
                else
                {
                    // Someone was hungry.
                    string localizedName = "";
                    ProtagonistHealthData mainCharacter = GetMainHealthData(hungryCharacters);
                    if(mainCharacter != null)
                    {
                        // The main character was hungry.
                        localizedName = GetNameForCharacter(mainCharacter.CharacterData);
                    }
                    else
                    {
                        // The side characters were hungry.
                        localizedName = GetNameForCharacter(hungryCharacters[0].CharacterData);
                    }

                    return LocalizationManager.Instance.GetLocalizedString(lastNightHotelNotEnoughFood, $"{localizedName}");
                }
            }

            case SleepMethod.Ship:
            {
                string localizedDays = $"{daysInCity}";
                if(stopoverLocation != null)
                {
                    string localizedStopover = NewGameManager.Instance.LocationManager.GetLocalizedName(stopoverLocation);
                    return LocalizationManager.Instance.GetLocalizedString(lastNightShipStopoverDay, localizedDays, localizedStopover);
                }
                else
                {
                    int randomIndex = Random.Range(0, lastNightShip.Length);
                    return LocalizationManager.Instance.GetLocalizedString(lastNightShip[randomIndex], localizedDays);
                }
            }
        }

        return "";
    }

    private ProtagonistHealthData GetMainHealthData(IEnumerable<ProtagonistHealthData> characters)
    {
        foreach(var character in characters)
        {
            if(character.CharacterData.isMainProtagonist)
            {
                return character;
            }
        }

        return null;
    }

    private string GetNameForCharacter(ProtagonistData data)
    {
        if(data.isMainProtagonist)
        {
            return LocalizationManager.Instance.GetLocalizedString(i);
        }

        return data.name;
    }

    private string GetLocalizedStolenItems(List<StolenItemInfo> items)
    {
        List<string> localizedItemNames = new List<string>(items.Select(item => {
            if (item.type == StolenItemType.Money)
            {
                return $"{item.money} {NewGameManager.Instance.CurrentCurrency}";
            }
            else
            {
                return LocalizationManager.Instance.GetLocalizedString(item.item.Name);
            }
        }));

        string result = "";
        for(int i = 0; i < localizedItemNames.Count; ++i)
        {
            result += localizedItemNames[i];
            if (i < localizedItemNames.Count - 2)
            {
                result += ", ";
            }
            else if (i == localizedItemNames.Count - 2)
            {
                string localizedAnd = LocalizationManager.Instance.GetLocalizedString(and);
                result += $" {localizedAnd} ";
            }
        }

        return result;
    }

    public void GenerateEntry()
    {
        string transportationInfo = GenerateTransportationInfo();
        Debug.Log(transportationInfo);

        string lastNight = GenerateLastNight();
        Debug.Log(lastNight);
    }
}
