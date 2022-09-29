using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullImagePage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Image image;

    public void SetData(DiaryPageData data)
    {
        image.sprite = data.image;
        image.preserveAspect = true;
    }
}
