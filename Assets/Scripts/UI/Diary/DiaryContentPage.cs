using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryContentPage : MonoBehaviour
{
    public DiaryContentPages ContentPages
    {
        get
        {
            return GetComponentInParent<DiaryContentPages>();
        }
    }

    private Animator animator;
    private Coroutine watchAnimationCoroutine;
    public event OnDiaryStatusChangedEvent onStatusChanged;
    private OpenStatus status;
    public OpenStatus Status
    {
        get { return status; }
        set
        {
            status = value;
            onStatusChanged?.Invoke(status);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenToLeft()
    {
        switch(Status)
        {
            case OpenStatus.Closed:
                gameObject.SetActive(true);
                Status = OpenStatus.Opening;
                animator.SetTrigger("OpenToLeft");
                watchAnimationCoroutine = StartCoroutine(WatchAnimation());
                break;
        }
    }

    public void OpenToRight()
    {
        switch(Status)
        {
            case OpenStatus.Closed:
                gameObject.SetActive(true);
                Status = OpenStatus.Opening;
                animator.SetTrigger("OpenToRight");
                watchAnimationCoroutine = StartCoroutine(WatchAnimation());
                break;
        }
    }

    public void CloseToLeft()
    {
        switch(Status)
        {
            case OpenStatus.Opened:
                Status = OpenStatus.Closing;
                animator.SetTrigger("CloseToLeft");
                watchAnimationCoroutine = StartCoroutine(WatchAnimation());
                break;
        }
    }

    public void CloseToRight()
    {
        switch(Status)
        {
            case OpenStatus.Opened:
                Status = OpenStatus.Closing;
                animator.SetTrigger("CloseToRight");
                watchAnimationCoroutine = StartCoroutine(WatchAnimation());
                break;
        }
    }

    private IEnumerator WatchAnimation()
    { 
        while((Status == OpenStatus.Closing && !animator.GetCurrentAnimatorStateInfo(0).IsName("DiaryContentPageClosed"))
            || (Status == OpenStatus.Opening && !animator.GetCurrentAnimatorStateInfo(0).IsName("DiaryContentPageOpened")))
        {
            yield return null;
        }

        watchAnimationCoroutine = null;

        switch (Status)
        {
            case OpenStatus.Opening:
                Status = OpenStatus.Opened;
                break;

            case OpenStatus.Closing:
                gameObject.SetActive(false);
                Status = OpenStatus.Closed;
                break;
        }
    }
}
