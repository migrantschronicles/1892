using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDayElisIslandPopup : EndDaySleepPopupBase
{
    public override bool CanClose { get { return true; } }

    private void Start()
    {
        CreatePortraits();
    }

    public void OnAccept()
    {
        LevelInstance.Instance.OnSleepInElisIsland(GetEndOfDayHealthData());
        LevelInstance.Instance.PopPopup();
    }
}
