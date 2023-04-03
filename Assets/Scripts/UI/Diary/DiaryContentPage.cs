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
    [SerializeField]
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
            return GetComponentInParent<DiaryContentPages>(true);
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
        animator.SetFloat("IsDoublePage", isDoublePage ? 1.0f : 0.0f);
    }

    private void OnEnable()
    {
        animator.SetFloat("IsDoublePage", isDoublePage ? 1.0f : 0.0f);
        switch(status)
        {
            case OpenStatus.Opened:
                // The diary was closed during a popup, so open immediately.
                animator.SetTrigger("OpenImmediately");
                break;
        }
    }

    public void OpenImmediately()
    {
        Debug.Assert(Status == OpenStatus.Closed);
        gameObject.SetActive(true);
        // Animator etc is still not activated, only next frame
        LevelInstance.Instance.StartCoroutine(OpenImmediatelyNextFrame());
    }

    private IEnumerator OpenImmediatelyNextFrame()
    {
        yield return null;
        animator.SetTrigger("OpenImmediately");
        Status = OpenStatus.Opened;
    }

    public void OpenToLeft()
    {
        switch(Status)
        {
            case OpenStatus.Closed:
                gameObject.SetActive(true);
                LevelInstance.Instance.StartCoroutine(OpenToLeftNextFrame());
                break;
        }
    }

    private IEnumerator OpenToLeftNextFrame()
    {
        yield return null;
        Status = OpenStatus.Opening;
        animator.SetTrigger("OpenToLeft");
        watchAnimationCoroutine = StartCoroutine(WatchAnimation());
    }

    public void OpenToRight()
    {
        switch(Status)
        {
            case OpenStatus.Closed:
                gameObject.SetActive(true);
                LevelInstance.Instance.StartCoroutine(OpenToRightNextFrame());
                break;
        }
    }

    private IEnumerator OpenToRightNextFrame()
    {
        yield return null;
        Status = OpenStatus.Opening;
        animator.SetTrigger("OpenToRight");
        watchAnimationCoroutine = StartCoroutine(WatchAnimation());
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

    public void DiaryOpenToLeft()
    {
        switch (Status)
        {
            case OpenStatus.Closed:
                gameObject.SetActive(true);
                Status = OpenStatus.Opening;
                animator.SetTrigger("DiaryOpenToLeft");
                watchAnimationCoroutine = StartCoroutine(WatchAnimation());
                break;
        }
    }

    public void DiaryCloseToRight()
    {
        switch (Status)
        {
            case OpenStatus.Opened:
                Status = OpenStatus.Closing;
                animator.SetTrigger("DiaryCloseToRight");
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
