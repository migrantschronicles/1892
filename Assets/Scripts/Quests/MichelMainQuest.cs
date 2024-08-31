using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MichelMainQuest : MonoBehaviour
{
    [SerializeField]
    private Quest familyMainQuest;
    [SerializeField]
    private int targetAmount = 100;

    private string targetLocation = "ElisIsland";

    private void Start()
    {
        if (NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter != CharacterType.Michel)
        {
            Destroy(this);
            return;
        }

        NewGameManager.Instance.QuestManager.onQuestFinished += OnQuestFinished;
        if(NewGameManager.Instance.QuestManager.HasQuest(familyMainQuest))
        {
            OnQuestStarted();
        }
        else
        {
            NewGameManager.Instance.QuestManager.onQuestAdded += OnQuestAdded;
        }
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onLocationChanged -= OnLocationChanged;
        }
    }

    private void OnQuestAdded(Quest quest)
    {
        if(quest == familyMainQuest)
        {
            OnQuestStarted();
        }
    }

    private void OnQuestFinished(Quest quest)
    {
        if(quest == familyMainQuest)
        {
        }
    }

    private void OnQuestStarted()
    {
        NewGameManager.Instance.onLocationChanged += OnLocationChanged;
    }

    private void OnQuestFailed()
    {
        NewGameManager.Instance.QuestManager.FailQuest(familyMainQuest);
        NewGameManager.Instance.onLocationChanged -= OnLocationChanged;
    }
    
    private void OnLocationChanged(string location)
    {
        if(location == targetLocation)
        { 
            if(NewGameManager.Instance.QuestManager.IsQuestActive(familyMainQuest))
            {
                if(NewGameManager.Instance.money >= targetAmount)
                {
                    OnQuestFinished();
                }
                else
                {
                    OnQuestFailed();
                }
            }
        }
    }

    private void OnQuestFinished()
    {
        NewGameManager.Instance.QuestManager.FinishQuest(familyMainQuest);
        NewGameManager.Instance.onLocationChanged -= OnLocationChanged;
    }
}
