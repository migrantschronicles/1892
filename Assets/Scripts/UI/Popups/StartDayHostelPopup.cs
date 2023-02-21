using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDayHostelPopup : MonoBehaviour, IPopup
{
    public delegate void OnStartDayEvent();
    public event OnStartDayEvent OnStartDay;

    public bool CanClose { get { return false; } }

    public void OnAccept()
    {
        OnStartDay?.Invoke();
    }
}
