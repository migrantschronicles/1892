using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using static UnityEngine.Rendering.DebugUI;

public enum GeneratedDiaryEntryPurpose
{
    NewCity,
    NewDay
}

public class DiaryEntryManager : MonoBehaviour
{
    [SerializeField]
    private DiaryEntry pfaffenthalEntry;
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
    private LocalizedString cholera;
    [SerializeField]
    private LocalizedString homesickness;
    [SerializeField]
    private LocalizedString pfaffenthal;
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
                    string localizedName;
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
                string localizedDays = $"{daysInCity + 1}";
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

        return GetEnumerationString(localizedItemNames);
    }

    class HealthProblem
    {
        public string characterName;
        public string sickness;
    }

    private string GenerateHealthStatus()
    {
        List<ProtagonistHealthData> characters = new List<ProtagonistHealthData>(NewGameManager.Instance.HealthStatus.Characters);

        List<HealthProblem> newProblems = new();
        for(int i = 0; i < characters.Count; ++i)
        {
            ProtagonistHealthData character = characters[i];
            if(character.CholeraStatus.DaysSick == 1)
            {
                // Got sick
                string localizedSickness = LocalizationManager.Instance.GetLocalizedString(cholera);
                newProblems.Add(new HealthProblem { characterName = character.CharacterData.name, sickness = localizedSickness });
                characters.RemoveAt(i);
                --i;
            }
            else if(character.HomesickessStatus.DaysSick == 1)
            {
                // Got homesick
                string localizedSickness = LocalizationManager.Instance.GetLocalizedString(homesickness);
                newProblems.Add(new HealthProblem { characterName = character.CharacterData.name, sickness = localizedSickness });
                characters.RemoveAt(i);
                --i;
            }
        }

        List<HealthProblem> existingProblems = new();
        for(int i = 0; i < characters.Count; ++i)
        {
            ProtagonistHealthData character = characters[i];
            if(character.CholeraStatus.DaysSick > 1)
            {
                // Still sick
                string localizedSickness = LocalizationManager.Instance.GetLocalizedString(cholera);
                existingProblems.Add(new HealthProblem { characterName = character.CharacterData.name, sickness = localizedSickness });
                characters.RemoveAt(i);
                --i;
            }
            else if(character.HomesickessStatus.DaysSick > 1)
            {
                // Still homesick
                string localizedSickness = LocalizationManager.Instance.GetLocalizedString(homesickness);
                existingProblems.Add(new HealthProblem { characterName = character.CharacterData.name, sickness = localizedSickness });
                characters.RemoveAt(i);
                --i;
            }
        }

        if(newProblems.Count == 0 && existingProblems.Count == 0)
        {
            return LocalizationManager.Instance.GetLocalizedString(healthNoProblems);
        }

        string result = "";
        if(newProblems.Count == 1)
        {
            result += LocalizationManager.Instance.GetLocalizedString(healthNewProblem, newProblems[0].characterName, newProblems[0].sickness);
        }
        else if(newProblems.Count > 1)
        {
            string characterNames = GetEnumerationString(new List<string>(newProblems.Select(problem => problem.characterName)));
            string sicknesses = GetEnumerationString(new List<string>(newProblems.Select(problem => problem.sickness)));
            result += LocalizationManager.Instance.GetLocalizedString(healthNewProblems, characterNames, sicknesses);
        }

        if(existingProblems.Count > 0)
        {
            if(result.Length > 0)
            {
                result += " ";
            }

            result += existingProblems
                .Select(problem => LocalizationManager.Instance.GetLocalizedString(healthExistingProblem, problem.characterName, problem.sickness))
                .Aggregate((a, b) => $"{a} {b}");
        }

        return result;
    }

    private string GetEnumerationString(List<string> values)
    {
        string result = "";
        for (int i = 0; i < values.Count; ++i)
        {
            result += values[i];
            if (i < values.Count - 2)
            {
                result += ", ";
            }
            else if (i == values.Count - 2)
            {
                string localizedAnd = LocalizationManager.Instance.GetLocalizedString(and);
                result += $" {localizedAnd} ";
            }
        }
        return result;
    }

    private string GenerateCity()
    {
        LocalizedString value = null;
        if (LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
        {
            switch(NewGameManager.Instance.DaysInCity)
            {
                case 0: value = shipDay1; break;
                case 1: value = shipDay2; break;
                case 2: value = shipDay3; break;
                case 3: value = shipDay4; break;
                case 4: value = shipDay5; break;
                case 5: value = shipDay6; break;
                case 6: value = shipDay7; break;
                case 7: value = shipDay8; break;
                case 8: value = shipDay9; break;
                case 9: value = shipDay10; break;
            }
        }
        else
        {
            string currentLocation = LevelInstance.Instance.LocationName;
            switch (currentLocation)
            {
                case "Luxembourg": value = luxembourg; break;
                case "Paris": value = paris; break;
                case "Brussels": value = brussels; break;
                case "LeHavre": value = lehavre; break;
                case "Rotterdam": value = rotterdam; break;
                case "Bremerhaven": value = bremerhaven; break;
                case "Antwerp": value = antwerp; break;
                case "Marseille": value = marseille; break;
                case "Hamburg": value = hamburg; break;
                case "Liverpool": value = liverpool; break;
                case "Southampton": value = southampton; break;
                case "Genoa": value = genoa; break;
                case "ElisIsland": value = elisIsland; break;
                case "NewYorkCity": value = newYorkCity; break;
                case "Chicago": value = chicago; break;
                case "Boston": value = boston; break;
                case "Milwaukee": value = milwaukee; break;
                case "VillageOfBelgium": value = villageOfBelgium; break;
                case "Minneapolis": value = minneapolis; break;
                case "Rollingstone": value = rollingstone; break;
                case "Dubuque": value = dubuque; break;
                case "StDonatus": value = stDonatus; break;
                case "Philadelphia": value = philadelphia; break;
            }
        }

        return LocalizationManager.Instance.GetLocalizedString(value);
    }

    private string GenerateText(GeneratedDiaryEntryPurpose purpose)
    {
        if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship || NewGameManager.Instance.ShipManager.IsStopoverDay)
        {
            if(NewGameManager.Instance.DaysInCity == 0)
            {
                // On first day, only city
                return LocalizationManager.Instance.GetLocalizedString(shipDay1);
            }

            string lastNight = GenerateLastNight();
            string healthStatus = GenerateHealthStatus();
            string city = GenerateCity();
            return $"{lastNight} {healthStatus} {city}";
        }
        else if(LevelInstance.Instance.LocationName == "Pfaffenthal")
        {
            // Special diary entry for pfaffenthal
            return LocalizationManager.Instance.GetLocalizedString(pfaffenthal);
        }
        else if(LevelInstance.Instance.LocationName == "ElisIsland")
        {
            // Special diary entry for elis island.
            return LocalizationManager.Instance.GetLocalizedString(elisIsland);
        }

        // Always generate transportation info.
        string transporationInfo = GenerateTransportationInfo();

        // Determine if it's a diary entry for when you arrived to a city or on a new day
        switch (purpose)
        {
            case GeneratedDiaryEntryPurpose.NewCity:
            {
                // The content for the city.
                string city = GenerateCity();

                return $"{transporationInfo} {city}";
            }

            case GeneratedDiaryEntryPurpose.NewDay:
            {
                // How last night was
                string lastNight = GenerateLastNight();
                // Health status
                string healthStatus = GenerateHealthStatus();

                return $"{transporationInfo} {lastNight} {healthStatus}";
            }
        }

        return "";
    }

    private DiaryEntry GetDiaryEntryForCity()
    {
        if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
        {
            switch(NewGameManager.Instance.DaysInCity)
            {
                case 0: return shipDay1Entry;
                case 1: return shipDay2Entry;
                case 2: return shipDay3Entry;
                case 3: return shipDay4Entry;
                case 4: return shipDay5Entry;
                case 5: return shipDay6Entry;
                case 6: return shipDay7Entry;
                case 7: return shipDay8Entry;
                case 8: return shipDay9Entry;
                case 9: return shipDay10Entry;
            }
        }
        else
        {
            string currentLocation = LevelInstance.Instance.LocationName;
            switch (currentLocation)
            {
                case "Pfaffenthal": return pfaffenthalEntry;
                case "Luxembourg": return luxembourgEntry;
                case "Paris": return parisEntry;
                case "Brussels": return brusselsEntry;
                case "LeHavre": return leHavreEntry;
                case "Rotterdam": return rotterdamEntry;
                case "Bremerhaven": return bremerhavenEntry;
                case "Antwerp": return antwerpEntry;
                case "Marseille": return marseilleEntry;
                case "Hamburg": return hamburgEntry;
                case "Liverpool": return liverpoolEntry;
                case "Southampton": return southamptonEntry;
                case "Genoa": return genoaEntry;
                case "ElisIsland": return elisIslandEntry;
                case "NewYorkCity": return newYorkCityEntry;
                case "Chicago": return chicagoEntry;
                case "Boston": return bostonEntry;
                case "Milwaukee": return milwaukeeEntry;
                case "VillageOfBelgium": return villageOfBelgiumEntry;
                case "Minneapolis": return minneapolisEntry;
                case "Rollingstone": return rollingstoneEntry;
                case "Dubuque": return dubuqueEntry;
                case "StDonatus": return stDonatusEntry;
                case "Philadelphia": return philadelphiaEntry;
            }
        }

        return null;
    }

    public DiaryEntryData GenerateEntry(GeneratedDiaryEntryPurpose purpose)
    {
        DiaryEntry diaryEntry = GetDiaryEntryForCity();
        if(!diaryEntry)
        {
            Debug.LogError($"Diary entry not found for {LevelInstance.Instance.LocationName}");
            return null;
        }

        // Create a copy to not modify the asset.
        DiaryEntryData diaryEntryData = new DiaryEntryData 
        { 
            entry = diaryEntry,
            leftPage = diaryEntry.leftPage.Clone(),
            rightPage = diaryEntry.rightPage.Clone()
        };

        string text = GenerateText(purpose);
        diaryEntryData.localizedText = text;
        diaryEntryData.date = NewGameManager.Instance.date;

        // Distribute text to different pages.
        List<Vector2> leftWeights = diaryEntryData.leftPage.prefab.GetComponent<IDiaryPage>().GetTextFieldWeights();
        List<Vector2> rightWeights = diaryEntryData.rightPage.prefab.GetComponent<IDiaryPage>().GetTextFieldWeights();
        if(text.Length < 250)
        {
            // If the text is really short, don't distribute it.
            if(leftWeights.Count > 0)
            {
                diaryEntryData.leftPage.Text = text;
            }
            else if(rightWeights.Count > 0)
            {
                diaryEntryData.rightPage.Text = text;
            }
        }
        else
        {
            // The text is long, so distribute the text.
            float sum = leftWeights.Select(weight => weight.x * weight.y).Sum() + rightWeights.Select(weight => weight.x * weight.y).Sum();
            int start = 0;
            for(int i = 0; i < leftWeights.Count; ++i)
            {
                float alpha = (leftWeights[i].x * leftWeights[i].y) / sum;
                int count = start + (int) (text.Length * alpha);

                // Find the next space.
                while (count < text.Length && !char.IsWhiteSpace(text[count]))
                {
                    ++count;
                }

                string partialText = text.Substring(start, count - start);
                if (i == 0)
                {
                    diaryEntryData.leftPage.Text = partialText;
                }
                else
                {
                    diaryEntryData.leftPage.Text2 = partialText;
                }
                start = count;
            }

            for(int i = 0; i < rightWeights.Count; ++i)
            {
                float alpha = (rightWeights[i].x * rightWeights[i].y) / sum;
                int count = start + (int)(text.Length * alpha);

                // Find the next space.
                while (count < text.Length && !char.IsWhiteSpace(text[count]))
                {
                    ++count;
                }

                string partialText = text.Substring(start, Mathf.Min(count, text.Length) - start);
                if (i == 0)
                {
                    diaryEntryData.rightPage.Text = partialText;
                }
                else
                {
                    diaryEntryData.rightPage.Text2 = partialText;
                }
                start = count;
            }
        }

        return diaryEntryData;
    }
}
