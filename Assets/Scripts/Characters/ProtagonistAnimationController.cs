using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProtagonistAnimState
{
    Neutral = 0,
    Angry = 1,
    Happy = 2,
    Hungry = 3,
    Sad = 4,
    Sick = 5
}

public class ProtagonistAnimationController : IAnimationController
{
    [SerializeField]
    private string protagonistName;

    private ProtagonistAnimState state = ProtagonistAnimState.Neutral;

    private void Start()
    {
        ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
        healthData.onHealthChanged += OnHealthChanged;
        OnHealthChanged(healthData);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
            healthData.onHealthChanged -= OnHealthChanged;
        }
    }

    private void OnEnable()
    {
        GetAnimator().SetInteger("State", (int)state);
    }

    private void OnHealthChanged(ProtagonistHealthData healthData)
    {
        if(healthData.CholeraStatus.IsSick)
        {
            SetAnimState(ProtagonistAnimState.Sick);
        }
        else if(healthData.HungryStatus.DaysWithoutEnoughFood >= 2)
        {
            SetAnimState(ProtagonistAnimState.Hungry);
        }
        else if(healthData.HomesickessStatus.Value >= 5.0f)
        {
            SetAnimState(ProtagonistAnimState.Sad);
        }
        else if(healthData.CholeraStatus.IsExposed || healthData.HungryStatus.DaysWithoutEnoughFood > 0 || healthData.HomesickessStatus.Value > 2.5f)
        {
            SetAnimState(ProtagonistAnimState.Neutral);
        }
        else
        {
            SetAnimState(ProtagonistAnimState.Happy);
        }
    }

    public void SetAnimState(ProtagonistAnimState newState)
    {
        if(state != newState)
        {
            state = newState;
            GetAnimator().SetInteger("State", (int)state);
        }
    }
}
