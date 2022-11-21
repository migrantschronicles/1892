using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiaryStatus
{
    Closed,
    Opening,
    Opened,
    Closing
}

public class Diary : MonoBehaviour
{
    public delegate void OnDiaryStatusChangedEvent(DiaryStatus status);
    public event OnDiaryStatusChangedEvent onDiaryStatusChanged;

    private DiaryStatus diaryStatus;
    public DiaryStatus Status
    {
        get { return diaryStatus; }
        set
        {
            diaryStatus = value;
            onDiaryStatusChanged?.Invoke(diaryStatus);
        }
    }

    public IEnumerable<LocationMarker> LocationMarkerObjects
    {
        get
        {
            ///@todo
            return new List<LocationMarker>();
        }
    }

    public void SetVisible(bool visible)
    {
        if(visible)
        {
            switch (Status)
            {
                case DiaryStatus.Closed:
                case DiaryStatus.Closing:
                    Status = DiaryStatus.Opening;
                    break;
            }
        }
        else
        {
            switch(Status)
            {
                case DiaryStatus.Opened:
                case DiaryStatus.Opening:
                    Status = DiaryStatus.Closing;
                    break;
            }
        }
    }

    public void OnOpeningAnimationFinished()
    {
        Debug.Assert(Status == DiaryStatus.Opening);
        Status = DiaryStatus.Opened;
    }

    public void OnClosingAnimationFinished()
    {
        Debug.Assert(Status == DiaryStatus.Closing);
        Status = DiaryStatus.Closed;
    }
}
