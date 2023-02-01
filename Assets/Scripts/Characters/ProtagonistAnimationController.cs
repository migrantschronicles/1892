using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtagonistAnimationController : IAnimationController
{
    [SerializeField]
    private string protagonistName;
    [SerializeField]
    private float angrySpeed = 1.0f;
    [SerializeField]
    private float happySpeed = 1.0f;
    [SerializeField]
    private float hungrySpeed = 1.0f;
    [SerializeField]
    private float sadSpeed = 1.0f;
    [SerializeField]
    private float sickSpeed = 1.0f;

    private void Start()
    {
        ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
        healthData.onHealthStateChanged += OnHealthStateChanged;
        OnHealthStateChanged(healthData);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
            healthData.onHealthChanged -= OnHealthStateChanged;
        }
    }

    private void OnEnable()
    {
        if (NewGameManager.Instance)
        {
            ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
            GetAnimator().SetInteger("State", (int)healthData.HealthState);
        }
        GetAnimator().SetFloat("AngrySpeed", angrySpeed);
        GetAnimator().SetFloat("HappySpeed", happySpeed);
        GetAnimator().SetFloat("HungrySpeed", hungrySpeed);
        GetAnimator().SetFloat("SadSpeed", sadSpeed);
        GetAnimator().SetFloat("SickSpeed", sickSpeed);
    }

    private void OnHealthStateChanged(ProtagonistHealthData healthData)
    {
        GetAnimator().SetInteger("State", (int)healthData.HealthState);
    }
}
