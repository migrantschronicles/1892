using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterDayEntry
{
    public int day;
    public Room room;
    public Vector3 position;
    public Vector3 scale = Vector3.one;
}

public class MoveCharacterOnDays : MonoBehaviour
{
    [SerializeField]
    private CharacterDayEntry[] entries;
    [SerializeField]
    private DialogButton dialogButton;

    private void Start()
    {
        NewGameManager.Instance.onNewDay += OnNewDay;
        SetDay(NewGameManager.Instance.DaysInCity);
    }

    private void OnNewDay()
    {
        SetDay(NewGameManager.Instance.DaysInCity);
    }

    private void SetDay(int day)
    {
        int entryIndex = 0;
        for(; entryIndex < entries.Length; ++entryIndex)
        {
            if (entries[entryIndex].day > day) 
            {
                break;
            }
        }

        if(--entryIndex >= 0)
        {
            CharacterDayEntry entry = entries[entryIndex];
            transform.SetParent(entry.room.Middleground.transform, false);
            dialogButton.transform.SetParent(entry.room.Interactables.transform, false);
            transform.localPosition = entry.position;
            transform.localScale = entry.scale;
        }
    }
}
