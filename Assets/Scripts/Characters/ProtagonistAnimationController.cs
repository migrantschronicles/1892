using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Animation controller for protagonists.
 * Protagonists have different animations for each health state they are in.
 */
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
    [SerializeField]
    private Vector2 stateAnimationTimeRange = new Vector2(5, 10);

    public string ProtagonistName { get { return protagonistName; } }
    private float stateAnimationTimer = -1.0f;
    private bool IsProtagonist { get => NewGameManager.Instance && NewGameManager.Instance.PlayerCharacterManager.HasPlayerCharacter(ProtagonistName); }

    private void Start()
    {
        if(IsProtagonist)
        {
            if(!LevelInstance.Instance.ShouldOverrideProtagonistAnimState)
            {
                ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
                healthData.onHealthStateChanged += OnHealthStateChanged;
                OnHealthStateChanged(healthData);
            }

            UpdateValues();
        }
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance && IsProtagonist && !LevelInstance.Instance.ShouldOverrideProtagonistAnimState)
        {
            ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
            healthData.onHealthStateChanged -= OnHealthStateChanged;
        }
    }

    private void OnEnable()
    {
        if(!IsProtagonist)
        {
            return;
        }

        UpdateValues();
    }

    private void UpdateValues()
    {
        if (LevelInstance.Instance.ShouldOverrideProtagonistAnimState)
        {
            GetAnimator().SetInteger("State", (int)LevelInstance.Instance.OverrideProtagonistAnimState);
        }
        else
        {
            if (NewGameManager.Instance)
            {
                ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
                GetAnimator().SetInteger("State", (int)healthData.HealthState);
            }
        }

        GetAnimator().SetFloat("AngrySpeed", angrySpeed);
        GetAnimator().SetFloat("HappySpeed", happySpeed);
        GetAnimator().SetFloat("HungrySpeed", hungrySpeed);
        GetAnimator().SetFloat("SadSpeed", sadSpeed);
        GetAnimator().SetFloat("SickSpeed", sickSpeed);
        stateAnimationTimer = UnityEngine.Random.Range(0.0f, stateAnimationTimeRange.y);
    }

    private void OnDisable()
    {
        stateAnimationTimer = -1.0f;
    }

    private void Update()
    {
        if(IsProtagonist && stateAnimationTimer > 0.0f)
        {
            stateAnimationTimer -= Time.deltaTime;
            if(stateAnimationTimer <= 0.0f)
            {
                GetAnimator().SetTrigger("TriggerStateAnimation");
                stateAnimationTimer = UnityEngine.Random.Range(stateAnimationTimeRange.x, stateAnimationTimeRange.y);
            }
        }
    }

    private void OnHealthStateChanged(ProtagonistHealthData healthData)
    {
        GetAnimator().SetInteger("State", (int)healthData.HealthState);
    }
}
