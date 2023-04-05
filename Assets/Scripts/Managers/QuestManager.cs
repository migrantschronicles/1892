using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private List<Quest> mainQuests = new ();
    private List<Quest> sideQuests = new ();
    private List<Quest> finishedMainQuests = new ();
    private List<Quest> finishedSideQuests = new ();

    public IEnumerable<Quest> MainQuests { get { return mainQuests; } }
    public IEnumerable<Quest> SideQuests { get { return sideQuests; } }
    public IEnumerable<Quest> FinishedMainQuests { get { return finishedMainQuests; } }
    public IEnumerable<Quest> FinishedSideQuests { get { return finishedSideQuests; } }
    public IEnumerable<Quest> MainQuestsIncludingFinished { get { return mainQuests.Concat(finishedMainQuests); } }
    public IEnumerable<Quest> SideQuestsIncludingFinished { get { return sideQuests.Concat(finishedSideQuests); } }

    public delegate void OnQuestAddedEvent(Quest quest);
    public event OnQuestAddedEvent onQuestAdded;

    public delegate void OnQuestFinishedEvent(Quest quest);
    public event OnQuestFinishedEvent onQuestFinished;

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

        if (HasQuest(quest))
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

    public void FinishQuest(Quest quest)
    {
        GetQuestList(quest.Type).Remove(quest);
        GetQuestList(quest.Type, true).Add(quest);
        OnQuestFinished(quest);
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

    public bool HasQuest(Quest quest, bool includeFinished = false)
    {
        return IsQuestActive(quest) || (includeFinished && IsQuestFinished(quest));
    }

    public bool IsQuestActive(Quest quest)
    {
        return GetQuestList(quest.Type).Contains(quest);
    }

    public bool IsQuestFinished(Quest quest)
    {
        return GetQuestList(quest.Type, true).Contains(quest);
    }

    private List<Quest> GetQuestList(QuestType type, bool finished = false)
    {
        switch (type)
        {
            case QuestType.SideQuest:
                return finished ? finishedSideQuests : sideQuests;

            case QuestType.MainQuest:
                return finished ? finishedMainQuests : mainQuests;
        }

        return null;
    }
}
