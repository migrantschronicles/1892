using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestEntry : MonoBehaviour
{
    [SerializeField]
    private Color finishedForegroundColor = Color.black;
    [SerializeField]
    private Color failedForegroundColor = Color.black;
    [SerializeField]
    private Text titleText;

    private Quest quest;
    public Quest Quest
    {
        get {  return quest; }
        set
        {
            if(quest)
            {
                quest.Title.StringChanged -= UpdateTitle;
            }

            quest = value;
            titleText.text = LocalizationManager.Instance.GetLocalizedString(quest.Title);
            quest.Title.StringChanged += UpdateTitle;
        }
    }

    private void OnDestroy()
    {
        if(quest)
        {
            quest.Title.StringChanged -= UpdateTitle;
        }
    }

    private void UpdateTitle(string title)
    {
        titleText.text = title;
    }

    public void MarkFinished()
    {
        titleText.color = finishedForegroundColor;
    }

    public void MarkFailed()
    {
        titleText.color = failedForegroundColor;
    }
}
