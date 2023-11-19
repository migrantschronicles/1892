using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperHealthPanel : MonoBehaviour
{
    [SerializeField]
    private string characterName;
    [SerializeField]
    private Text characterNameText;
    [SerializeField]
    private InputField homesicknessValue;

    public ProtagonistHealthData HealthData
    {
        get
        {
            ProtagonistHealthData data = NewGameManager.Instance.HealthStatus.GetHealthStatus(characterName);
            Debug.Assert(data != null);
            return data;
        }
    }

    private void Awake()
    {
        if(characterNameText)
        {
            characterNameText.text = characterName;
        }
    }

    public void SetHungry()
    {
        HealthData.HungryStatus.Dev_SetHungry(true);
        HealthData.Dev_OnHealthChanged();
    }

    public void SetNotHungry()
    {
        HealthData.HungryStatus.Dev_SetHungry(false);
        HealthData.Dev_OnHealthChanged();
    }

    public void SetHomesick()
    {
        if(int.TryParse(homesicknessValue.text, out int value))
        {
            if(value >= 1 && value <= 10)
            {
                HealthData.HomesickessStatus.Dev_SetValue(value);
                HealthData.Dev_OnHealthChanged();
            }
        }
    }

    public void SetHealthy()
    {
        HealthData.CholeraStatus.Dev_SetHealthy();
        HealthData.Dev_OnHealthChanged();
    }

    public void SetExposed()
    {
        HealthData.CholeraStatus.Dev_SetExposed();
        HealthData.Dev_OnHealthChanged();
    }

    public void SetSick()
    {
        HealthData.CholeraStatus.Dev_SetSick();
        HealthData.Dev_OnHealthChanged();
    }

    public void SetSeasick()
    {
        HealthData.SeasicknessStatus.Dev_SetSeasick(true);
        HealthData.Dev_OnHealthChanged();
    }

    public void SetNotSeasick()
    {
        HealthData.SeasicknessStatus.Dev_SetSeasick(false);
        HealthData.Dev_OnHealthChanged();
    }
}
