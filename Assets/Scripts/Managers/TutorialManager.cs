using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialAction
{
    None,
    GetReady,
    Pack_OrganizeBelongings,
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public List<TutorialAction> remainingActions { get; private set; } = new ();
    private TutorialAction activeAction = TutorialAction.None;

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
        foreach(TutorialAction action in Enum.GetValues (typeof (TutorialAction)))
        {
            if(action != TutorialAction.None)
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

    public bool HasCompleted(TutorialAction action)
    {
        return !remainingActions.Contains(action);
    }

    public void ActivateAction(TutorialAction action)
    {
        if(HasCompleted(action) || IsActionActive(action))
        {
            return;
        }

        activeAction = action;
    }

    public void CompleteAction(TutorialAction action)
    {
        if(!IsActionActive(action))
        {
            return;
        }

        remainingActions.Remove(activeAction);
        activeAction = TutorialAction.None;
    }

    public bool IsActionActive(TutorialAction action)
    {
        return activeAction == action;
    }
}
