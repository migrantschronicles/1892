using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        LevelInstance.Instance.IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;
        ResetAnimation();
    }
    
    private void OnDiaryStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Opening)
        {
            ResetAnimation();
        }
    }

    private void ResetAnimation()
    {
        // GetComponentInParent does not work because it does not include inactive game objects
        DiaryContentPage page = GetComponentsInParent<DiaryContentPage>(true)[0];
        if (page.Status == OpenStatus.Opened)
        {
            OnPageStatusChanged(OpenStatus.Opened);
        }
        else
        {
            // Maybe hide or set frame 0
            gameObject.SetActive(false);
            page.onStatusChanged += OnPageStatusChanged;
        }
    }

    private void OnPageStatusChanged(OpenStatus status)
    {
        if (status == OpenStatus.Opened)
        {
            gameObject.SetActive(true);
            DiaryContentPage page = GetComponentInParent<DiaryContentPage>();
            page.onStatusChanged -= OnPageStatusChanged;
            animator.SetTrigger("Play");
        }
    }
}
