using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;

    public delegate void OnDiaryOpenedEvent();
    public event OnDiaryOpenedEvent onDiaryOpened;
    public delegate void OnDiaryClosedEvent();
    public event OnDiaryClosedEvent onDiaryClosed;

    private bool opened;

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
        opened = visible;
        diaryAnimator.SetBool("Opened", visible);
        StartCoroutine(BroadcastStateEvents());
    }

    private IEnumerator BroadcastStateEvents()
    {
        while((opened && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryOpened")) 
            || (!opened && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryClosed")))
        {
            yield return null;
        }

        if(opened)
        {
            onDiaryOpened?.Invoke();
        }
        else
        {
            onDiaryClosed?.Invoke();
        }
    }
}
