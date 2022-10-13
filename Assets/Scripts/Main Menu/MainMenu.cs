using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * To open the diary, call MainMenu::OpenDiary with the page you want opened as the argument.
 * To close the diary, call MainMenu::CloseDiary.
 * 
 * MAIN MENU PAGES
 * The prefab MainMenuPage is a prefab for a double sided page of the diary in the main menu.
 * Place a new page under Main Menu > Diary > Content.
 * You can create groups with empty game objects (e.g. NewGame for the new game button of the cover).
 * Once added, you can add your content to the left or right page.
 * If you want to switch pages and have a button somewhere, you can add a call to MainMenu::OpenNextPage or OpenPreviousPage,
 * depending on if you want to scroll to the right or left, with the animator of the new page as the argument.
 */
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

    public void OnStartGame()
    {
        SceneManager.LoadScene("Pfaffenthal");
    }
}
