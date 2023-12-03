using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum TutorialFeature
{
    None,
    ClockUnlocked,
    DiaryUnlocked,
    DialogDecision,
    EndOfDay,
    EndOfDay_Hostel,
    EndOfDay_Outside,
    EndOfDay_Ship,
    ClockButton
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private TutorialBlur blur;
    public TutorialBlur Blur
    {
        get
        {
            if(blur == null)
            {
                blur = FindObjectOfType<TutorialBlur>();
            }

            return blur;
        }
    }

    public List<TutorialFeature> CompletedFeatures { get; private set; } = new ();

    private void Awake()
    {
        if (Instance == null)
        {
            transform.SetParent(null, false);
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Clear()
    {
        CompletedFeatures.Clear();
    }

    public void LoadFromSaveGame(SaveData saveGame)
    {
        CompletedFeatures.Clear();
        CompletedFeatures.AddRange(saveGame.completedTutorialFeatures);
    }

    public bool HasCompleted(TutorialFeature action)
    {
        return CompletedFeatures.Contains(action);
    }

    public void CompleteFeature(TutorialFeature action)
    {
        CompletedFeatures.Add(action);
        switch(action)
        {
            case TutorialFeature.DiaryUnlocked:
            case TutorialFeature.ClockUnlocked:
                LevelInstance.Instance.UI.UpdateUIElements();
                break;
        }
    }
}
