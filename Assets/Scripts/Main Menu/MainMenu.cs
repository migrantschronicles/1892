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
    public Transform scene; // Used for switching between diary and language selection interface.
    [SerializeField]
    private Vector3 menuInitialPos = new Vector3(0, 0, 0);
    [SerializeField]
    private Vector3 languageSelectionPos = new Vector3(0, -1202, 0);


    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Animator coverAnimator;
    [SerializeField]
    private Animator newGamePage0;
    [SerializeField]
    private AudioClip openDiaryClip;
    [SerializeField]
    private AudioClip closeDiaryClip;
    [SerializeField]
    private AudioClip pageClip;
    [SerializeField]
    private AudioClip[] musicClips;

    private Animator currentPage;

    private void Start()
    {
        Application.targetFrameRate = 30;
        AudioManager.Instance.PlayMusic(musicClips);
    }

    public void OpenLanguageSelection() 
    {
        while (Vector3.Distance(scene.transform.localPosition, languageSelectionPos) > 0.01f)
            scene.localPosition = Vector3.Lerp(scene.transform.position, languageSelectionPos, 0.5f * Time.deltaTime);
    }

    public void CloseLanguageSelection()
    {
        while (Vector3.Distance(scene.transform.localPosition, menuInitialPos) > 0.01f)
            scene.localPosition = Vector3.Lerp(scene.transform.position, menuInitialPos, 0.5f * Time.deltaTime);
    }

    public void OpenDiary()
    {
        OpenDiary(null);
    }

    public void OpenDiary(Animator page)
    {
        diaryAnimator.SetBool("Opened", true);
        coverAnimator.SetBool("DiaryOpened", true);
        AudioManager.Instance.PlayFX(openDiaryClip);

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
        AudioManager.Instance.PlayFX(closeDiaryClip);
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
        AudioManager.Instance.PlayFX(pageClip);
    }

    public void OpenPreviousPage(Animator prevPage)
    {
        CloseCurrentPage(true);
        OpenPage(prevPage, false);
        ScrollBackward();
        AudioManager.Instance.PlayFX(pageClip);
    }
}
