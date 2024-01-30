using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private void OnEnable()
    {
        StartCoroutine(UpdateLayout());
    }

    private IEnumerator UpdateLayout()
    {
        yield return null;

        // If the quests are added while the diary is not visible, the Content Size Fitter does not calculate correctly the first frame.
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void OnQuestFinished(Quest quest)
    {
        for (int i = quests.Count - 1; i >= 0; --i)
        {
            QuestEntry questEntry = quests[i];
            if (!questEntry || !questEntry.gameObject)
            {
                quests.RemoveAt(i);
            }
            else if (questEntry.Quest == quest)
            {
                Destroy(questEntry.gameObject);
                quests.RemoveAt(i);
            }
        }
    }

    public void OnQuestFailed(Quest quest)
    {
        for(int i = quests.Count - 1; i >= 0; --i)
        {
            QuestEntry questEntry = quests[i];
            if(!questEntry || !questEntry.gameObject)
            {
                quests.RemoveAt(i);
            }
            else if(questEntry.Quest == quest)
            {
                Destroy(questEntry.gameObject);
                quests.RemoveAt(i);
            }
        }
    }
}
