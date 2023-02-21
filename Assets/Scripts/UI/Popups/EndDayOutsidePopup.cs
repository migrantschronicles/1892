using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDayOutsidePopup : EndDaySleepPopupBase
{
    private void Start()
    {
        CreatePortraits();
        InitFoodCounter();
    }
}
