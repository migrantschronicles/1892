using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum MainMenuDiaryState
{
    Away,
    Closed,
    Opened
}

public class MainMenuDiary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Diary diary;

    private MainMenuDiaryState diaryState = MainMenuDiaryState.Away;

    public Animator DiaryAnimator { get { return diaryAnimator; } }
    public Diary Diary { get { return diary; } }
    public bool IsHistoryMode { get; private set; }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch(status)
        {
            case OpenStatus.Opening:
                diaryState = MainMenuDiaryState.Opened;
                diaryAnimator.SetInteger("State", (int) diaryState);
                StartCoroutine(WaitForAnimationEvents());
                break;

            case OpenStatus.Closing:
                diaryState = MainMenuDiaryState.Closed;
                diaryAnimator.SetInteger("State", (int)diaryState);
                StartCoroutine(WaitForAnimationEvents());
                break;
        }
    }

    private IEnumerator WaitForAnimationEvents()
    {
        while ((diary.Status == OpenStatus.Opening && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("Opened"))
            || (diary.Status == OpenStatus.Closing && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed")))
        {
            yield return null;
        }

        switch (diary.Status)
        {
            case OpenStatus.Opening:
                diary.OnOpeningAnimationFinished();
                break;

            case OpenStatus.Closing:
                diary.OnClosingAnimationFinished();
                break;
        }
    }

    public void SetState(MainMenuDiaryState newState)
    {
        if(newState == diaryState)
        {
            return;
        }

        if((diaryState == MainMenuDiaryState.Away && newState == MainMenuDiaryState.Opened) || 
            (diaryState == MainMenuDiaryState.Opened && newState == MainMenuDiaryState.Away))
        {
            Debug.LogWarning("Cannot open or put away directly");
            return;
        }

        diaryState = newState;
        diaryAnimator.SetInteger("State", (int)diaryState);
    }

    /**
     * Checks if the animator is in the state given.
     */
    public bool IsAnimatorState(MainMenuDiaryState state)
    {
        string animatorState = state.ToString();
        return diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName(animatorState);
    }

    /**
     * Called from MainMenuDiary_AwayToClosed animation
     */
    public void OnDiaryToLanguageSelection()
    {
        MainMenuController.Instance.OnDiaryToLanguageSelection();
    }

    public void Anim_StartPageAnimation()
    {
        diary.Anim_StartPageAnimation();
    }

    public void Anim_EndPageAnimation()
    {
        diary.Anim_EndPageAnimation();
    }

    public void OpenPage(DiaryContentPage page)
    {
        if(diary.Status != OpenStatus.Opened && diary.Status != OpenStatus.Closed || diary.IsAnimationInProgress)
        {
            return;
        }

        switch(diaryState)
        {
            case MainMenuDiaryState.Closed:
            {
                diary.SetOpened(page);
                break;
            }

            case MainMenuDiaryState.Opened:
            {
                diary.OpenPage(page);
                break;
            }
        }
    }

    public void OpenPreviousPageOrClose()
    {
        if(diary.Status != OpenStatus.Opened || diary.IsAnimationInProgress)
        {
            return;
        }

        if(!diary.OpenPrevPageOfContentPages())
        {
            diary.SetOpened(false);
        }
    }

    public void OpenNextPage()
    {
        if(diary.Status != OpenStatus.Opened || diary.IsAnimationInProgress)
        {
            return;
        }

        diary.OpenNextPageOfContentPages();
    }
    
    public void SetIsHistoryMode(bool bHistoryMode)
    {
        IsHistoryMode = bHistoryMode;
    }

    public void StartNewGame(string username)
    {
        LevelManager.Instance.StartNewGame(username, IsHistoryMode);
    }
}
