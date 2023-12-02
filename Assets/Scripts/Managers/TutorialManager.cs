using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialFeature
{
    None,
    ClockUnlocked,
    DiaryUnlocked
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public List<TutorialFeature> remainingActions { get; private set; } = new ();

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

    public void SetAllRemaining()
    {
        foreach(TutorialFeature action in Enum.GetValues (typeof (TutorialFeature)))
        {
            if(action != TutorialFeature.None)
            {
                remainingActions.Add(action);
            }
        }
    }

    public void LoadFromSaveGame(SaveData saveGame)
    {
        remainingActions.Clear();
        remainingActions.AddRange(saveGame.remainingTutorialActions);
    }

    public bool HasCompleted(TutorialFeature action)
    {
        return !remainingActions.Contains(action);
    }

    public void CompleteAction(TutorialFeature action)
    {
        remainingActions.Remove(action);
        switch(action)
        {
            case TutorialFeature.DiaryUnlocked:
            case TutorialFeature.ClockUnlocked:
                LevelInstance.Instance.UI.UpdateUIElements();
                break;
        }
    }
}
