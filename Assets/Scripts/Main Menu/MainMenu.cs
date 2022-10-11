using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Animator coverAnimator;

    public void ContinueGame()
    {
        OpenDiary();
    }

    public void NewGame()
    {
        OpenDiary();
    }

    public void NewGamePlus()
    {
        OpenDiary();
    }

    public void StartTutorial()
    {
        OpenDiary();
    }

    public void OpenSettings()
    {
        OpenDiary();
    }

    private void OpenDiary()
    {
        diaryAnimator.SetBool("Opened", true);
        coverAnimator.SetBool("DiaryOpened", true);
    }

    public void CloseDiary()
    {
        diaryAnimator.SetBool("Opened", false);
        coverAnimator.SetBool("DiaryOpened", false);
    }

    public void ScrollBackward()
    {
        diaryAnimator.SetTrigger("ScrollBackward");
    }

    public void ScrollForward()
    {
        diaryAnimator.SetTrigger("ScrollForward");
    }
}
