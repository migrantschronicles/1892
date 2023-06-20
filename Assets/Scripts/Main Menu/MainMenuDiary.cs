using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private MainMenuDiaryState diaryState = MainMenuDiaryState.Away;

    public Animator DiaryAnimator { get { return diaryAnimator; } }

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
}
