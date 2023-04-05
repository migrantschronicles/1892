using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamilyMainQuest : MonoBehaviour
{
    [SerializeField]
    private Quest familyMainQuest;
    [SerializeField]
    private DateTime failedDate = new DateTime(1892, 6, 24);

    private void Start()
    {
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
    }
}
