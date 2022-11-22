using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestsPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject entryPrefab;

    private List<QuestEntry> quests = new List<QuestEntry>();

    public void OnQuestAdded(Quest quest)
    {
        GameObject newEntryGO = Instantiate(entryPrefab, transform);
        QuestEntry newEntry = newEntryGO.GetComponent<QuestEntry>();
        newEntry.Quest = quest;
        quests.Add(newEntry);
    }

    public void OnQuestFinished(Quest quest)
    {
        foreach(QuestEntry entry in quests)
        {
            if(entry.Quest == quest)
            {
                entry.MarkFinished();
                return;
            }
        }

        Debug.Assert(false);
    }
}
