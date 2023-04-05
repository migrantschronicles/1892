using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLog : MonoBehaviour
{
    [SerializeField]
    private QuestsPanel mainQuestsPanel;
    [SerializeField]
    private QuestsPanel sideQuestsPanel;

    private void Start()
    {
        NewGameManager.Instance.QuestManager.onQuestAdded += OnQuestAdded;
        NewGameManager.Instance.QuestManager.onQuestFinished += OnQuestFinished;
        NewGameManager.Instance.QuestManager.onQuestFailed += OnQuestFailed;

        foreach(Quest quest in NewGameManager.Instance.QuestManager.FinishedMainQuests)
        {
            mainQuestsPanel.OnQuestAdded(quest);
            mainQuestsPanel.OnQuestFinished(quest);
        }

        foreach (Quest quest in NewGameManager.Instance.QuestManager.FinishedSideQuests)
        {
            sideQuestsPanel.OnQuestAdded(quest);
            sideQuestsPanel.OnQuestFinished(quest);
        }

        foreach (Quest quest in NewGameManager.Instance.QuestManager.MainQuests)
        {
            mainQuestsPanel.OnQuestAdded(quest);
        }

        foreach (Quest quest in NewGameManager.Instance.QuestManager.SideQuests)
        {
            sideQuestsPanel.OnQuestAdded(quest);
        }

        foreach(Quest quest in NewGameManager.Instance.QuestManager.FailedMainQuests)
        {
            mainQuestsPanel.OnQuestAdded(quest);
            mainQuestsPanel.OnQuestFailed(quest);
        }

        foreach (Quest quest in NewGameManager.Instance.QuestManager.FailedSideQuests)
        {
            sideQuestsPanel.OnQuestAdded(quest);
            sideQuestsPanel.OnQuestFailed(quest);
        }
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.QuestManager.onQuestAdded -= OnQuestAdded;
            NewGameManager.Instance.QuestManager.onQuestFinished -= OnQuestFinished;
        }
    }

    private QuestsPanel GetPanelForQuest(Quest quest)
    {
        return quest.Type == QuestType.MainQuest ? mainQuestsPanel : sideQuestsPanel;
    }

    private void OnQuestAdded(Quest quest)
    {
        GetPanelForQuest(quest).OnQuestAdded(quest);
    }

    private void OnQuestFinished(Quest quest)
    {
        GetPanelForQuest(quest).OnQuestFinished(quest);
    }

    private void OnQuestFailed(Quest quest)
    {
        GetPanelForQuest(quest).OnQuestFailed(quest);
    }
}
