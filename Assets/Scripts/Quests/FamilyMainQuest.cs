using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamilyMainQuest : MonoBehaviour
{
    [SerializeField]
    private Quest familyMainQuest;

    private DateTime failedDate = new DateTime(1892, 8, 15);
    private string targetLocation = "ElisIsland";

    private void Start()
    {
        if (NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter != CharacterType.Elis)
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
            NewGameManager.Instance.onDateChanged -= OnDateChanged;
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
            NewGameManager.Instance.onDateChanged -= OnDateChanged;
        }
    }

    private void OnQuestStarted()
    {
        NewGameManager.Instance.onDateChanged += OnDateChanged;
        NewGameManager.Instance.onLocationChanged += OnLocationChanged;
    }

    private void OnDateChanged(DateTime date)
    {
        if(date >= failedDate)
        {
            OnQuestFailed();
        }
    }

    private void OnQuestFailed()
    {
        NewGameManager.Instance.QuestManager.FailQuest(familyMainQuest);
        NewGameManager.Instance.onDateChanged -= OnDateChanged;
        NewGameManager.Instance.onLocationChanged -= OnLocationChanged;
    }
    
    private void OnLocationChanged(string location)
    {
        if(location == targetLocation)
        { 
            if(NewGameManager.Instance.QuestManager.IsQuestActive(familyMainQuest))
            {
                OnQuestFinished();
            }
        }
    }

    private void OnQuestFinished()
    {
        NewGameManager.Instance.QuestManager.FinishQuest(familyMainQuest);
        NewGameManager.Instance.onDateChanged -= OnDateChanged;
        NewGameManager.Instance.onLocationChanged -= OnLocationChanged;
    }
}
