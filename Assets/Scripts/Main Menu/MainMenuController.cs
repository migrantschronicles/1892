using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private List<GameObject> diaryObjects;
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
    }

    public void OnLanguageSelected(Language language)
    {
        state = MainMenuState.Diary;

        Animator cameraAnimator = Camera.main.GetComponent<Animator>();
        if(cameraAnimator != null)
        {
            cameraAnimator.SetInteger("State", (int)state);
            StartCoroutine(WaitForTransitionToDiaryEnd(cameraAnimator));
        }
    }

    private IEnumerator WaitForTransitionToDiaryEnd(Animator animator)
    {
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Diary"))
        {
            yield return null;
        }

        while(!mainMenuDiary.IsAnimatorState(MainMenuDiaryState.Closed))
        {
            yield return null;
        }

        OnLanguageSelectionToDiaryFinished();
    }

    private void OnLanguageSelectionToDiaryFinished()
    {
        diaryObjects.ForEach(go => go.SetActive(true));
    }

    public void OnBackToLanguageSelection()
    {
        state = MainMenuState.LanguageSelection;
        diaryObjects.ForEach(go => go.SetActive(false));
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
}
