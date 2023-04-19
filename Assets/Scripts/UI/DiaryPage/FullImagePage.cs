using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullImagePage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Image image;

    public IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            ImageElementAnimator.FromImage(this, image)
        };
    }

    public void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        image.sprite = data.image;
        image.preserveAspect = true;
    }
}
