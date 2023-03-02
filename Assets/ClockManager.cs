using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockManager : MonoBehaviour
{

    public Transform hourHandle;
    public Transform minuteHandle;
    public Transform knob;

    public float minuteOffset = 13.193f;
    public float hourOffset = 101.601f;

    void Start() 
    {
        OnTimeChangedEvent(NewGameManager.Instance.hour, NewGameManager.Instance.minutes);
        NewGameManager.Instance.onTimeChanged += OnTimeChangedEvent;        
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onTimeChanged -= OnTimeChangedEvent;
        }
    }

    private void OnTimeChangedEvent(int hour, int minutes)
    {
        minuteHandle.rotation = Quaternion.Euler(0, 0, minuteHandle.rotation.z - (minutes * (360 / 60)) + minuteOffset);
        hourHandle.rotation = Quaternion.Euler(0, 0, hourHandle.rotation.z - (hour * (360 / 12) + (minutes * 0.5f)) + hourOffset);

        // Moving the knob with the day.
        Vector3 startPoint = new Vector3(175, 161, 0);
        Vector3 endPoint = new Vector3(205, 99.6f, 0);
        Vector3 controlPoint1 = new Vector3(175, 35.6f, 0);
        //Vector3 controlPoint2 = new Vector3(140, 10.5f, 0);


        float t = (hour + minutes / 60f) / NewGameManager.Instance.hoursPerDay; // Scale "hours" to a value between 0 and 1
        knob.localPosition = Vector3.Lerp(Vector3.Lerp(startPoint, controlPoint1, t), Vector3.Lerp(controlPoint1, endPoint, t), t);
    }

}
