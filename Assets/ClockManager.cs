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

    private void OnTimeChangedEvent(int hour, int minutes)
    {
        minuteHandle.rotation = Quaternion.Euler(0, 0, minuteHandle.rotation.z - (minutes * (360 / 60)) + minuteOffset);
        hourHandle.rotation = Quaternion.Euler(0, 0, hourHandle.rotation.z - (hour * (360 / 12) + (minutes * 0.5f)) + hourOffset);

        // Moving the knob with the day.
        /*float knobY = 92f - ((92f - 10.5f) / NewGameManager.Instance.hoursPerDay);
        float knobX = 120f;
        if (hour <= (NewGameManager.Instance.hoursPerDay/2))
        {
            knobX = 120f + hour * ((140f - 120f) / (NewGameManager.Instance.hoursPerDay/2));
        }
        else { knobX = 140f - hour * ((140f - 120f) / (NewGameManager.Instance.hoursPerDay / 2)); }

        knob.SetLocalPositionAndRotation(new Vector3(knobX, knobY, 0), knob.rotation);*/

        Vector3 startPoint = new Vector3(120, 92, 0);
        Vector3 endPoint = new Vector3(120, 10.5f, 0);
        Vector3 controlPoint1 = new Vector3(160, 51.25f, 0);
        //Vector3 controlPoint2 = new Vector3(140, 10.5f, 0);


        float t = (hour + minutes / 60f) / 13.0f; // Scale "hours" to a value between 0 and 1
        knob.localPosition = Vector3.Lerp(Vector3.Lerp(startPoint, controlPoint1, t), Vector3.Lerp(controlPoint1, endPoint, t), t);
    }

}
