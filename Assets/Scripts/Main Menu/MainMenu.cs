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
    private bool inLanguageSelection = false;
    public float transitionSpeed = 0.9f;
    private bool readyToStartAnimations = false; // To indicate if language selection is done and main menu (book) is ready to be activated
    public Transform mainCamera;


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

    public Animator ENBook;
    public Animator LUXBook;

    private void Start()
    {
        Application.targetFrameRate = 30;
        AudioManager.Instance.PlayMusic(musicClips);
        inLanguageSelection = true;
    }

    private void Update()
    {
        if (inLanguageSelection && Vector3.Distance(mainCamera.localPosition, languageSelectionPos) > 0.01f) mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, languageSelectionPos, transitionSpeed * Time.deltaTime);
        else if (!inLanguageSelection && Vector3.Distance(scene.transform.localPosition, menuInitialPos) > 0.01f) mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, menuInitialPos, transitionSpeed * Time.deltaTime);
        else if (Vector3.Distance(scene.transform.localPosition, menuInitialPos) <= 0.01f) readyToStartAnimations = true;
    }

    public void OpenLanguageSelection() 
    {
        inLanguageSelection = true;
    }

    public void CloseLanguageSelection()
    {
        inLanguageSelection = false;
    }

    IEnumerator CloseLanguageSelectionWithDelay(float seconds) 
    {
        yield return new WaitForSeconds(seconds);
        CloseLanguageSelection();
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(StartMainMenu());
    }

    public IEnumerator StartMainMenu() 
    {
        diaryAnimator.gameObject.SetActive(true);
        diaryAnimator.SetTrigger("BookToPosition");
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < coverAnimator.gameObject.transform.childCount; i++) { coverAnimator.gameObject.transform.GetChild(i).gameObject.SetActive(true); }
    }

    public void SelectLanguage(string language) 
    {
        // Apply it to GameManager (For later)
        StartCoroutine(CloseLanguageSelectionWithDelay(0.5f));        
    }

    public void OpenDiary()
    {
        OpenDiary(null);
    }

    public void OpenDiary(Animator page)
    {
        diaryAnimator.SetTrigger("BookToPositionOpen");

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
