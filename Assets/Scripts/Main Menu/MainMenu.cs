using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Animator coverAnimator;
    [SerializeField]
    private Animator newGamePage0;

    private Animator currentPage;

    public void OpenDiary()
    {
        OpenDiary(null);
    }

    public void OpenDiary(Animator page)
    {
        diaryAnimator.SetBool("Opened", true);
        coverAnimator.SetBool("DiaryOpened", true);

        if(page)
        {
            OpenPage(page, true);
        }
    }

    public void CloseDiary()
    {
        diaryAnimator.SetBool("Opened", false);
        coverAnimator.SetBool("DiaryOpened", false);
        CloseCurrentPage(true);
    }

    public void ScrollBackward()
    {
        diaryAnimator.SetTrigger("ScrollBackward");
    }

    public void ScrollForward()
    {
        diaryAnimator.SetTrigger("ScrollForward");
    }

    private void CloseCurrentPage(bool fromRight)
    {
        if(currentPage)
        {
            currentPage.SetTrigger(fromRight ? "CloseRight" : "CloseLeft");
            currentPage = null;
        }
    }

    private void OpenPage(Animator page, bool fromRight)
    {
        currentPage = page;
        currentPage.SetTrigger(fromRight ? "OpenRight" : "OpenLeft");
    }

    public void OpenNextPage(Animator nextPage)
    {
        CloseCurrentPage(false);
        OpenPage(nextPage, true);
        ScrollForward();
    }

    public void OpenPreviousPage(Animator prevPage)
    {
        CloseCurrentPage(true);
        OpenPage(prevPage, false);
        ScrollBackward();
    }
}
