using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryContentPage : MonoBehaviour
{
    [SerializeField]
    private bool isDoublePage = false;
    [SerializeField]
    private GameObject leftPage;
    [SerializeField]
    private GameObject rightPage;
    [SerializeField]
    private GameObject doublePage;

    private Animator animator;
    private Coroutine watchAnimationCoroutine;
    public event OnDiaryStatusChangedEvent onStatusChanged;
    private OpenStatus status;

    public GameObject LeftPage { get { return leftPage; } }
    public GameObject RightPage { get { return rightPage; } }
    public GameObject DoublePage { get { return doublePage; } }

    public bool IsAnimationInProgress
    {
        get
        {
            return !(Status == OpenStatus.Opened || Status == OpenStatus.Closed);
        }
    }

    public OpenStatus Status
    {
        get { return status; }
        set
        {
            status = value;
            onStatusChanged?.Invoke(status);
        }
    }

    public DiaryContentPages ContentPages
    {
        get
        {
            return GetComponentInParent<DiaryContentPages>();
        }
    }

    /**
     * @return True if this is the first page within the same hierarchy level 
     */
    public bool IsFirstPageOfContentPages
    {
        get
        {
            return transform.GetSiblingIndex() == 0;
        }
    }

    /**
     * @return True if this is the last page within the same hierarchy level 
     */
    public bool IsLastPageOfContentPages
    {
        get
        {
            return transform.GetSiblingIndex() == transform.parent.childCount - 1;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("IsDoublePage", isDoublePage ? 1.0f : 0.0f);
    }

    private void OnEnable()
    {
        animator.SetFloat("IsDoublePage", isDoublePage ? 1.0f : 0.0f);
    }

    public void OpenImmediately()
    {
        Debug.Assert(Status == OpenStatus.Closed);
        gameObject.SetActive(true);
        animator.SetTrigger("OpenImmediately");
        Status = OpenStatus.Opened;
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

    public void CloseImmediately()
    {
        Debug.Assert(Status == OpenStatus.Opened);
        animator.SetTrigger("CloseImmediately");
        gameObject.SetActive(false);
        Status = OpenStatus.Closed;
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
