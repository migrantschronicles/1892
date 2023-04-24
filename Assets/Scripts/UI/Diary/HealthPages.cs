using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPages : MonoBehaviour
{
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private Button nextPageButton;
    [SerializeField]
    private AudioClip nextPageClip;
    [SerializeField]
    private AudioClip prevPageClip;
    [SerializeField]
    private DiaryContentPages contentPages;

    private void Awake()
    {
        prevPageButton.onClick.AddListener(OpenPrevDoublePage);
        nextPageButton.onClick.AddListener(OpenNextDoublePage);
        contentPages.onActiveStatusChanged += OnContentPagesActiveStatusChanged;
    }

    public void OnContentPagesActiveStatusChanged(bool visible)
    {
        if(visible)
        {
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        if(NewGameManager.Instance.wantsEndGame)
        {
            nextPageButton.gameObject.SetActive(false);
            prevPageButton.gameObject.SetActive(false);
        }
        else
        {
            DiaryContentPage currentPage = LevelInstance.Instance.IngameDiary.Diary.CurrentPage;
            if(currentPage && currentPage.ContentPages == contentPages)
            {
                prevPageButton.gameObject.SetActive(!currentPage.IsFirstPageOfContentPages);
                nextPageButton.gameObject.SetActive(!currentPage.IsLastPageOfContentPages);
            }
        }
    }

    private void OpenPrevDoublePage()
    {
        if(LevelInstance.Instance.IngameDiary.Diary.OpenPrevPageOfContentPages())
        {
            AudioManager.Instance.PlayFX(prevPageClip);
        }
    }

    private void OpenNextDoublePage()
    {
        if(LevelInstance.Instance.IngameDiary.Diary.OpenNextPageOfContentPages())
        {
            AudioManager.Instance.PlayFX(nextPageClip);
        }
    }
}
