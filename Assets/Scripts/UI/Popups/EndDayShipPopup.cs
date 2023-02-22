using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDayShipPopup : EndDaySleepPopupBase
{
    public bool CanClose { get { return NewGameManager.Instance.RemainingTime > 0; } }
    public IPopup.OnCanCloseChangedEvent OnCanCloseChanged;

    private void Start()
    {
        CreatePortraits();
    }

    private void OnEnable()
    {
        NewGameManager.Instance.onTimeChanged += OnTimeChanged;
        OnTimeChanged(NewGameManager.Instance.hour, NewGameManager.Instance.minutes);
    }

    private void OnDisable()
    {
        if (NewGameManager.Instance)
        {
            NewGameManager.Instance.onTimeChanged -= OnTimeChanged;
        }
    }

    private void OnTimeChanged(int hour, int minute)
    {
        if (!CanClose)
        {
            OnCanCloseChanged?.Invoke(this, false);
        }
    }

    public void OnAccept()
    {
        LevelInstance.Instance.OnSleepInShip(GetEndOfDayHealthData());
    }
}
