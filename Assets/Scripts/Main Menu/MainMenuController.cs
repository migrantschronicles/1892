using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    enum MainMenuState
    {
        LanguageSelection,
        Diary
    }

    [SerializeField]
    private AudioClip[] musicClips;
    [SerializeField]
    private Button backToLanguageSelectionButton;
    [SerializeField]
    public SpriteRenderer FlagEnglish;
    [SerializeField]
    private SpriteRenderer FlagLouxembourgish;
    [SerializeField]
    private MainMenuDiary mainMenuDiary;
    private MainMenuState state;

    public static MainMenuController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        AudioManager.Instance.PlayMusic(musicClips);

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            AudioManager.Instance.MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            AudioManager.Instance.SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
        }

        mainMenuDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    private void OnDiaryStatusChanged(OpenStatus openStatus)
    {
        switch(openStatus)
        {
            case OpenStatus.Closed:
                if(state == MainMenuState.Diary)
                {
                    backToLanguageSelectionButton.gameObject.SetActive(true);
                }
                break;

            default:
                backToLanguageSelectionButton.gameObject.SetActive(false);
                break;
        }
    }

    public void OnLanguageSelected(Language language)
    {
        LocalizationManager.Instance.ChangeLanguage(language);
        state = MainMenuState.Diary;

        Animator cameraAnimator = Camera.main.GetComponent<Animator>();
        if(cameraAnimator != null)
        {
            cameraAnimator.SetInteger("State", (int)state);
            StartCoroutine(WaitForTransitionToDiaryEnd(cameraAnimator, language));
        }
    }

    private IEnumerator WaitForTransitionToDiaryEnd(Animator animator, Language language)
    {
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Diary"))
        {
            yield return null;
        }

        while(!mainMenuDiary.IsAnimatorState(MainMenuDiaryState.Closed))
        {
            yield return null;
        }

        OnLanguageSelectionToDiaryFinished(language);
    }

    private void OnLanguageSelectionToDiaryFinished(Language language)
    {
        backToLanguageSelectionButton.gameObject.SetActive(true);
        FlagEnglish.gameObject.SetActive(language == Language.English);
        FlagLouxembourgish.gameObject.SetActive(language == Language.Luxembourgish);
    }

    public void OnBackToLanguageSelection()
    {
        state = MainMenuState.LanguageSelection;
        backToLanguageSelectionButton.gameObject.SetActive(false);
        mainMenuDiary.SetState(MainMenuDiaryState.Away);
    }

    /**
     * Called from MainMenuCamera_LanguageSelectionToDiary animation.
     */
    public void OnSetDiaryToClosed()
    {
        switch(state)
        {
            case MainMenuState.Diary:
            {
                mainMenuDiary.SetState(MainMenuDiaryState.Closed);
                break;
            }
        }
    }

    /**
     * Called from the MainMenuDiary_ClosedToAway animation.
     */
    public void OnDiaryToLanguageSelection()
    {
        switch (state)
        {
            case MainMenuState.LanguageSelection:
            { 
                Animator cameraAnimator = Camera.main.GetComponent<Animator>();
                if (cameraAnimator != null)
                {
                    cameraAnimator.SetInteger("State", (int)state);
                }
                break;
            }
        }
    }

    public void OpenPage(DiaryContentPage page)
    {
        switch(state)
        {
            case MainMenuState.Diary:
                mainMenuDiary.OpenPage(page);
                break;
        }
    }
}
