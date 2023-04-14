using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public enum QuestListType
    {
        Added,
        Finished,
        Failed
    }

    [SerializeField]
    private Quest[] quests;

    private List<Quest> mainQuests = new ();
    private List<Quest> sideQuests = new ();
    private List<Quest> finishedMainQuests = new ();
    private List<Quest> finishedSideQuests = new ();
    private List<Quest> failedMainQuests = new();
    private List<Quest> failedSideQuests = new();

    public IEnumerable<Quest> MainQuests { get { return mainQuests; } }
    public IEnumerable<Quest> SideQuests { get { return sideQuests; } }
    public IEnumerable<Quest> FinishedMainQuests { get { return finishedMainQuests; } }
    public IEnumerable<Quest> FinishedSideQuests { get { return finishedSideQuests; } }
    public IEnumerable<Quest> FailedMainQuests { get { return failedMainQuests; } }
    public IEnumerable<Quest> FailedSideQuests { get { return failedSideQuests; } }

    public delegate void OnQuestAddedEvent(Quest quest);
    public event OnQuestAddedEvent onQuestAdded;

    public delegate void OnQuestFinishedEvent(Quest quest);
    public event OnQuestFinishedEvent onQuestFinished;

    public delegate void OnQuestFailedEvent(Quest quest);
    public event OnQuestFailedEvent onQuestFailed;

#if UNITY_EDITOR
    private void ValidateQuest(Quest quest)
    {
        if (string.IsNullOrWhiteSpace(quest.Id))
        {
            Debug.LogError($"{quest.name} has no id");
        }

        if (quest.Title == null || quest.Title.IsEmpty)
        {
            Debug.LogError($"{quest.name} has no title set");
        }
    }
#endif

    public bool AddQuest(Quest quest)
    {
#if UNITY_EDITOR
        ValidateQuest(quest);
#endif

        if (HasQuest(quest, true, true))
        {
            return false;
        }

        switch (quest.Type)
        {
            case QuestType.MainQuest:
                mainQuests.Add(quest);
                break;

            case QuestType.SideQuest:
                sideQuests.Add(quest);
                break;
        }

        OnQuestAdded(quest);
        return true;
    }

    private void OnQuestAdded(Quest quest)
    {
        onQuestAdded?.Invoke(quest);

        if (EvaluateQuestFinishedCondition(quest))
        {
            // Quest is already finished
            FinishQuest(quest);
        }
        else if (!quest.FinishedCondition.IsEmpty())
        {
            // Listen to changes to conditions.
            NewGameManager.Instance.conditions.AddOnConditionsChanged(quest.FinishedCondition.GetAllConditions(), OnQuestConditionsChanged, quest);
        }
    }

    private void OnQuestConditionsChanged(object context)
    {
        Quest quest = (Quest)context;
        if (EvaluateQuestFinishedCondition(quest))
        {
            FinishQuest(quest);
        }
    }

    public bool FinishQuest(Quest quest)
    {
        if(GetQuestList(quest.Type).Remove(quest))
        {
            GetQuestList(quest.Type, QuestListType.Finished).Add(quest);
            OnQuestFinished(quest);
            return true;
        }

        return false;
    }

    private void OnQuestFinished(Quest quest)
    {
        onQuestFinished?.Invoke(quest);
    }

    private bool EvaluateQuestFinishedCondition(Quest quest)
    {
        if (quest.FinishedCondition.IsEmpty())
        {
            return false;
        }

        return quest.FinishedCondition.Test();
    }

    public bool FailQuest(Quest quest)
    {
        if(GetQuestList(quest.Type).Remove(quest))
        {
            GetQuestList(quest.Type, QuestListType.Failed).Add(quest);
            OnQuestFailed(quest);
            return true;
        }

        return false;
    }

    private void OnQuestFailed(Quest quest)
    {
        onQuestFailed?.Invoke(quest);
    }

    public bool HasQuest(Quest quest, bool includeFinished = false, bool includeFailed = false)
    {
        return IsQuestActive(quest) || (includeFinished && IsQuestFinished(quest)) || (includeFailed && IsQuestFailed(quest));
    }

    public bool IsQuestActive(Quest quest)
    {
        return GetQuestList(quest.Type).Contains(quest);
    }

    public bool IsQuestFinished(Quest quest)
    {
        return GetQuestList(quest.Type, QuestListType.Finished).Contains(quest);
    }

    public bool IsQuestFailed(Quest quest)
    {
        return GetQuestList(quest.Type, QuestListType.Failed).Contains(quest);
    }

    private List<Quest> GetQuestList(QuestType type, QuestListType listType = QuestListType.Added)
    {
        switch (type)
        {
            case QuestType.SideQuest:
                switch(listType)
                {
                    case QuestListType.Added: return sideQuests;
                    case QuestListType.Finished: return finishedSideQuests;
                    case QuestListType.Failed: return failedSideQuests;
                }
                break;

            case QuestType.MainQuest:
                switch (listType)
                {
                    case QuestListType.Added: return mainQuests;
                    case QuestListType.Finished: return finishedMainQuests;
                    case QuestListType.Failed: return failedMainQuests;
                }
                break;
        }

        return null;
    }

    public Quest GetQuestById(string id)
    {
        return quests.FirstOrDefault(x => x.Id == id);
    }
}
