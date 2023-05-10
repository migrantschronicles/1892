using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum QuestType
{
    SideQuest,
    MainQuest
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest", order = 1)]
public class Quest : ScriptableObject
{
    [SerializeField, Tooltip("The name / identifier for this quest")]
    private string id;
    [SerializeField, Tooltip("The description shown in the quest log")]
    private LocalizedString title;
    [SerializeField, Tooltip("The conditions it sets when the quest is finished.")]
    private SetCondition[] setFinishedConditions;
    [SerializeField, Tooltip("The condition that need to be met that the quest is considered finished")]
    private DialogCondition finishedCondition;
    [SerializeField, Tooltip("The type of the quest")]
    private QuestType type;
    [SerializeField, Tooltip("Fail the quest as soon as you leave one of these cities. Can also be Europe/America/Ship")]
    private string[] failIfYouLeave;
    [SerializeField, Tooltip("Fail the quest as soon as you get to on of these cities. Can also be Europe/America/Ship")]
    private string[] failIfYouGetTo;

    public string Id { get { return id; } }
    public LocalizedString Title { get { return title; } }
    public IEnumerable<SetCondition> SetFinishedConditions { get { return setFinishedConditions; } }
    public DialogCondition FinishedCondition { get { return finishedCondition; } }
    public QuestType Type { get { return type; } }
    public IEnumerable<string> FailIfYouLeave { get { return failIfYouLeave; } }
    public IEnumerable<string> FailIfYouGetTo { get { return failIfYouGetTo; } }
}
