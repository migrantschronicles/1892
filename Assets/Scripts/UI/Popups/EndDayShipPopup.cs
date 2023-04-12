using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDayShipPopup : EndDaySleepPopupBase
{
    public override bool CanClose { get { return true; } }

    private void Start()
    {
        CreatePortraits();
    }

    public void OnAccept()
    {
        LevelInstance.Instance.OnSleepInShip(GetEndOfDayHealthData());
        LevelInstance.Instance.PopPopup();
    }
}
