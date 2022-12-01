using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockManager : MonoBehaviour
{

    public Transform hourHandle;
    public Transform minuteHandle;

    public float minuteOffset = 13.193f;
    public float hourOffset = 101.601f;

    void Start() 
    {
        OnTimeChangedEvent(NewGameManager.Instance.hour, NewGameManager.Instance.minutes);
        NewGameManager.Instance.onTimeChanged += OnTimeChangedEvent;        
    }

    private void OnTimeChangedEvent(int hour, int minutes)
    {
        minuteHandle.rotation = Quaternion.Euler(0, 0, minuteHandle.rotation.z - (minutes * (360 / 60)) + minuteOffset);
        hourHandle.rotation = Quaternion.Euler(0, 0, hourHandle.rotation.z - (hour * (360 / 12) + (minutes * 0.5f)) + hourOffset);

    }

}
