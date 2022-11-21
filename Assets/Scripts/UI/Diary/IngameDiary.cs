using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameDiary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Diary diary;
    [SerializeField]
    private DiaryContentPage defaultPage;

    public Diary Diary { get { return diary; } }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch(status)
        {
            case OpenStatus.Opening:
                diaryAnimator.SetBool("Opened", true);
                StartCoroutine(WaitForAnimationEvents());
                break;

            case OpenStatus.Closing:
                diaryAnimator.SetBool("Opened", false);
                StartCoroutine(WaitForAnimationEvents());
                break;
        }
    }

    private IEnumerator WaitForAnimationEvents()
    {
        while((diary.Status == OpenStatus.Opening && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryOpened"))
            || (diary.Status == OpenStatus.Closing && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryClosed")))
        {
            yield return null;
        }

        switch(diary.Status)
        {
            case OpenStatus.Opening:
                diary.OnOpeningAnimationFinished();
                break;

            case OpenStatus.Closing:
                diary.OnClosingAnimationFinished();
                break;
        }
    }

    public void SetOpened(bool opened)
    {
        if(opened)
        {
            diary.SetOpened(defaultPage);
        }
        else
        {
            diary.SetOpened(opened);
        }
    }
}
