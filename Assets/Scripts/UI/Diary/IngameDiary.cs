using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameDiary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Diary diary;

    public Diary Diary { get { return diary; } }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    private void OnDiaryStatusChanged(DiaryStatus status)
    {
        switch(status)
        {
            case DiaryStatus.Opening:
                diaryAnimator.SetBool("Opened", true);
                StartCoroutine(WaitForAnimationEvents());
                break;

            case DiaryStatus.Closing:
                diaryAnimator.SetBool("Opened", false);
                StartCoroutine(WaitForAnimationEvents());
                break;
        }
    }

    private IEnumerator WaitForAnimationEvents()
    {
        while((diary.Status == DiaryStatus.Opening && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryOpened"))
            || (diary.Status == DiaryStatus.Closing && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryClosed")))
        {
            yield return null;
        }

        switch(diary.Status)
        {
            case DiaryStatus.Opening:
                diary.OnOpeningAnimationFinished();
                break;

            case DiaryStatus.Closing:
                diary.OnClosingAnimationFinished();
                break;
        }
    }

    public void SetVisible(bool visible)
    {
        diary.SetVisible(visible);
    }
}
