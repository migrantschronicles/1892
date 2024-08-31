using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class EndGameDiaryEntryData
{
    public string technicalName;
    public bool hasHomesicknessVariant;
    public LocalizedString neutral;
    public LocalizedString positive;
    public LocalizedString negative;
}

[System.Serializable]
public class DiaryEntryLocationBase
{
    public DiaryEntry diaryEntry;
    public LocalizedString text;
}

[System.Serializable]
public class DiaryEntryLocation
{
    public string locationName;
    public DiaryEntryLocationBase data;
}

[System.Serializable]
public class DiaryEntryShip
{
    public int day;
    public DiaryEntryLocationBase data;
}

[System.Serializable]
public class DiaryEntryContainer
{
    public DiaryEntryLocation[] locations;
    public DiaryEntryShip[] shipDays;
    public LocalizedString[] transportationFirstDay;
    public LocalizedString transportationOtherDays;
    public LocalizedString lastNightHotelEnoughFood;
    public LocalizedString lastNightHotelNotEnoughFood;
    public LocalizedString lastNightOutsideEnoughFoodNothingStolen;
    public LocalizedString lastNightOutsideNotEnoughFoodNothingStolen;
    public LocalizedString lastNightOutsideEnoughFoodStolen;
    public LocalizedString lastNightOutsideNotEnoughFoodStolen;
    public LocalizedString[] lastNightShip;
    public LocalizedString lastNightShipStopoverDay;
    public LocalizedString healthNoProblems;
    public LocalizedString healthNewProblem;
    public LocalizedString healthNewProblems;
    public LocalizedString healthExistingProblem;
    public EndGameDiaryEntryData[] endGameEntries;

    public DiaryEntryLocation GetEntryForLocation(string location)
    {
        return locations.First(l => l.locationName == location);
    }

    public DiaryEntryShip GetEntryForShip(int day)
    {
        return shipDays.First(s => s.day == day);
    }

    public EndGameDiaryEntryData GetEndGameEntry(string technicalName)
    {
        return endGameEntries.FirstOrDefault(data => data.technicalName == technicalName);
    }
}

[System.Serializable]
public class ElisDiaryEntryContainer
{
    public DiaryEntryContainer data;
}

[System.Serializable]
public class PunnelsDiaryEntryContainer
{
    public DiaryEntryContainer data;
}

[System.Serializable]
public class MichelDiaryEntryContainer
{
    public DiaryEntryContainer data;
}

/**
 * Generates the diary entries.
 */
public class DiaryEntryManager : MonoBehaviour
{
    [SerializeField]
    private LocalizedString and;
    [SerializeField]
    private LocalizedString i;
    [SerializeField]
    private LocalizedString cholera;
    [SerializeField]
    private LocalizedString homesickness;

    [SerializeField]
    private ElisDiaryEntryContainer elisEntries;
    [SerializeField]
    private PunnelsDiaryEntryContainer punnelsEntries;
    [SerializeField]
    private MichelDiaryEntryContainer michelEntries;

    public bool IsPositiveEnding 
    { 
        get
        {
            return NewGameManager.Instance.HealthStatus.Characters.Max(data => data.HomesickessStatus.Value) <= 5.0f;
        }
    }

    private string GenerateTransportationInfo(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        string localizedLocation = NewGameManager.Instance.LocationManager.GetLocalizedName(info.locationName);
        if (info.daysInCity == 0)
        {
            // The day of the arrival
            int randomIndex = Random.Range(0, container.transportationFirstDay.Length);
            string localizedMethod = NewGameManager.Instance.TransportationManager.GetLocalizedMethod(info.lastTransportationMethod);
            return LocalizationManager.Instance.GetLocalizedString(container.transportationFirstDay[randomIndex], localizedLocation, localizedMethod);
        }
        else
        {
            // Other days
            return LocalizationManager.Instance.GetLocalizedString(container.transportationOtherDays, localizedLocation);
        }
    }

    private string GenerateLastNight(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        switch(info.lastSleepMethod)
        {
            case SleepMethod.Outside:
            {
                if(info.hungryCharacters.Count == 0)
                {
                    if(info.lastStolenItems.Count == 0)
                    {
                        // Nothing got stolen, no one is hungry.
                        return LocalizationManager.Instance.GetLocalizedString(container.lastNightOutsideEnoughFoodNothingStolen);
                    }
                    else
                    {
                        // Something got stolen, but no one is hungry.
                        Currency currency = NewGameManager.Instance.LocationManager.GetCurrencyForLocation(info.locationName);
                        string localizedItems = GetLocalizedStolenItems(info.lastStolenItems, currency);
                        return LocalizationManager.Instance.GetLocalizedString(container.lastNightOutsideEnoughFoodStolen, localizedItems);
                    }
                }
                else
                {
                    if(info.lastStolenItems.Count == 0)
                    {
                        // Nothing got stolen, but some one is hungry.
                        return LocalizationManager.Instance.GetLocalizedString(container.lastNightOutsideNotEnoughFoodNothingStolen);
                    }
                    else
                    {
                        // Something got stolen and someone is hungry.
                        Currency currency = NewGameManager.Instance.LocationManager.GetCurrencyForLocation(info.locationName);
                        string localizedItems = GetLocalizedStolenItems(info.lastStolenItems, currency);
                        return LocalizationManager.Instance.GetLocalizedString(container.lastNightOutsideNotEnoughFoodStolen, localizedItems);
                    }
                }
            }

            case SleepMethod.Hotel:
            {
                if(info.hungryCharacters.Count == 0)
                {
                    // Everyone had enough food.
                    return LocalizationManager.Instance.GetLocalizedString(container.lastNightHotelEnoughFood);
                }
                else
                {
                    // Someone was hungry.
                    string localizedName;
                    ProtagonistData mainCharacter = GetMainHealthData(info.hungryCharacters);
                    if(mainCharacter != null)
                    {
                        // The main character was hungry.
                        localizedName = GetNameForCharacter(mainCharacter);
                    }
                    else
                    {
                        // The side characters were hungry.
                        localizedName = GetNameForCharacter(info.hungryCharacters[0]);
                    }

                    return LocalizationManager.Instance.GetLocalizedString(container.lastNightHotelNotEnoughFood, $"{localizedName}");
                }
            }

            case SleepMethod.Ship:
            {
                string localizedDays = $"{info.daysInCity + 1}";
                // Stopovers are always on day 2
                if(!string.IsNullOrEmpty(info.stopoverLocation) && info.daysInCity == 1)
                {
                    string localizedStopover = NewGameManager.Instance.LocationManager.GetLocalizedName(info.stopoverLocation);
                    return LocalizationManager.Instance.GetLocalizedString(container.lastNightShipStopoverDay, localizedDays, localizedStopover);
                }
                else
                {
                    int randomIndex = Random.Range(0, container.lastNightShip.Length);
                    return LocalizationManager.Instance.GetLocalizedString(container.lastNightShip[randomIndex], localizedDays);
                }
            }
        }

        return "";
    }

    private ProtagonistData GetMainHealthData(IEnumerable<ProtagonistData> characters)
    {
        foreach(var character in characters)
        {
            if(character.isMainProtagonist)
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

    private string GetLocalizedStolenItems(List<StolenItemInfo> items, Currency currency)
    {
        List<string> localizedItemNames = new List<string>(items.Select(item => {
            if (item.type == StolenItemType.Money)
            {
                return $"{item.money} {currency}";
            }
            else
            {
                return LocalizationManager.Instance.GetLocalizedString(item.item.Name);
            }
        }));

        return GetEnumerationString(localizedItemNames);
    }

    private string GetLocalizedHealthProblemType(HealthProblemType type)
    {
        switch(type)
        {
            case HealthProblemType.Cholera: return LocalizationManager.Instance.GetLocalizedString(cholera);
            case HealthProblemType.Homesickness: return LocalizationManager.Instance.GetLocalizedString(homesickness);
        }

        return "NONE";
    }

    private string GenerateHealthStatus(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        if(info.newHealthProblems.Count == 0 && info.existingHealthProblems.Count == 0)
        {
            return LocalizationManager.Instance.GetLocalizedString(container.healthNoProblems);
        }

        string result = "";
        if(info.newHealthProblems.Count == 1)
        {
            string localizedName = info.newHealthProblems[0].character.name;
            string localizedSickness = GetLocalizedHealthProblemType(info.newHealthProblems[0].sickness);
            result += LocalizationManager.Instance.GetLocalizedString(container.healthNewProblem, localizedName, localizedSickness);
        }
        else if(info.newHealthProblems.Count > 1)
        {
            string characterNames = GetEnumerationString(new List<string>(info.newHealthProblems.Select(problem => problem.character.name)));
            string sicknesses = GetEnumerationString(new List<string>(info.newHealthProblems.Select(problem => 
                GetLocalizedHealthProblemType(problem.sickness))));
            result += LocalizationManager.Instance.GetLocalizedString(container.healthNewProblems, characterNames, sicknesses);
        }

        if(info.existingHealthProblems.Count > 0)
        {
            if(result.Length > 0)
            {
                result += " ";
            }

            result += info.existingHealthProblems
                .Select(problem => 
                    LocalizationManager.Instance.GetLocalizedString(container.healthExistingProblem, problem.character.name, 
                        GetLocalizedHealthProblemType(problem.sickness)))
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

    private string GenerateCity(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        LocalizedString value = null;
        if (info.levelMode == LevelInstanceMode.Ship)
        {
            value = container.GetEntryForShip(info.daysInCity).data.text;
        }
        else
        {
            value = container.GetEntryForLocation(info.locationName).data.text;
        }

        return LocalizationManager.Instance.GetLocalizedString(value);
    }

    private string GenerateText(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        if(info.purpose == GeneratedDiaryEntryPurpose.EndGame)
        {
            // Generate end game entry
            EndGameDiaryEntryData data = container.GetEndGameEntry(info.endGameEntryTechnicalName);
            if(data != null)
            {
                LocalizedString localizedString = data.hasHomesicknessVariant ? (IsPositiveEnding ? data.positive : data.negative) : data.neutral;
                return LocalizationManager.Instance.GetLocalizedString(localizedString);
            }

            Debug.LogError($"Cannot find end game diary entry for {info.endGameEntryTechnicalName}");
            return "";
        }

        if(info.levelMode == LevelInstanceMode.Ship || info.isStopoverDay)
        {
            if(info.daysInCity == 0)
            {
                // On first day, only city
                return LocalizationManager.Instance.GetLocalizedString(container.GetEntryForShip(0).data.text);
            }

            string lastNight = GenerateLastNight(info, container);
            string healthStatus = GenerateHealthStatus(info, container);
            string city = GenerateCity(info, container);
            return $"{lastNight} {healthStatus} {city}";
        }
        else if(info.locationName == "Pfaffenthal")
        {
            // Special diary entry for pfaffenthal
            return LocalizationManager.Instance.GetLocalizedString(container.GetEntryForLocation("Pfaffenthal").data.text);
        }
        else if(info.locationName == "Wormeldange")
        {
            return LocalizationManager.Instance.GetLocalizedString(container.GetEntryForLocation("Wormeldange").data.text);
        }
        else if(info.locationName == "Weicherdange")
        {
            return LocalizationManager.Instance.GetLocalizedString(container.GetEntryForLocation("Weicherdange").data.text);
        }
        else if(info.locationName == "ElisIsland")
        {
            // Special diary entry for elis island.
            return LocalizationManager.Instance.GetLocalizedString(container.GetEntryForLocation("ElisIsland").data.text);
        }

        // Always generate transportation info.
        string transporationInfo = GenerateTransportationInfo(info, container);

        // Determine if it's a diary entry for when you arrived to a city or on a new day
        switch (info.purpose)
        {
            case GeneratedDiaryEntryPurpose.NewCity:
            {
                // The content for the city.
                string city = GenerateCity(info, container);

                return $"{transporationInfo} {city}";
            }

            case GeneratedDiaryEntryPurpose.NewDay:
            {
                // How last night was
                string lastNight = GenerateLastNight(info, container);
                // Health status
                string healthStatus = GenerateHealthStatus(info, container);

                return $"{transporationInfo} {lastNight} {healthStatus}";
            }
        }

        return "";
    }

    private DiaryEntry GetDiaryEntryForCity(DiaryEntryInfo info, DiaryEntryContainer container)
    {
        if(info.levelMode == LevelInstanceMode.Ship)
        {
            return container.GetEntryForShip(info.daysInCity).data.diaryEntry;
        }
        else
        {
            return container.GetEntryForLocation(info.locationName).data.diaryEntry;
        }
    }

    private DiaryEntryContainer GetEntryContainer()
    {
        switch(NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis: return elisEntries.data;
            case CharacterType.Punnels: return punnelsEntries.data;
            case CharacterType.Michel: return michelEntries.data;
        }

        return null;
    }

    public DiaryEntryData GenerateEntry(DiaryEntryInfo info)
    {
        DiaryEntry diaryEntry = GetDiaryEntryForCity(info, GetEntryContainer());
        if(!diaryEntry)
        {
            Debug.LogError($"Diary entry not found for {info.locationName}");
            return null;
        }

        // Create a copy to not modify the asset.
        DiaryEntryData diaryEntryData = new DiaryEntryData
        {
            entry = diaryEntry,
            leftPage = diaryEntry.leftPage.Clone(),
            rightPage = diaryEntry.rightPage.Clone(),
            info = info,
            date = info.Date
        };

        UpdateDiaryEntry(diaryEntryData);

        return diaryEntryData;
    }

    public void UpdateDiaryEntry(DiaryEntryData diaryEntryData)
    {
        string text = GenerateText(diaryEntryData.info, GetEntryContainer());

        // Distribute text to different pages.
        List<Vector2> leftWeights = diaryEntryData.leftPage.prefab.GetComponent<IDiaryPage>().GetTextFieldWeights();
        List<Vector2> rightWeights = diaryEntryData.rightPage.prefab.GetComponent<IDiaryPage>().GetTextFieldWeights();
        if (text.Length < 250)
        {
            // If the text is really short, don't distribute it.
            if (leftWeights.Count > 0)
            {
                diaryEntryData.leftPage.Text = text;
            }
            else if (rightWeights.Count > 0)
            {
                diaryEntryData.rightPage.Text = text;
            }
        }
        else
        {
            // The text is long, so distribute the text.
            float sum = leftWeights.Select(weight => weight.x * weight.y).Sum() + rightWeights.Select(weight => weight.x * weight.y).Sum();
            int start = 0;
            for (int i = 0; i < leftWeights.Count; ++i)
            {
                float alpha = (leftWeights[i].x * leftWeights[i].y) / sum;
                int count = start + (int)(text.Length * alpha);

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

            for (int i = 0; i < rightWeights.Count; ++i)
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
    }
}
