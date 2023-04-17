using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageBottomPage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text text;

    public IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            TextElementAnimator.FromText(this, text),
            ImageElementAnimator.FromImage(this, image)
        };
    }

    public void SetData(DiaryPageData data)
    {
        image.sprite = data.image;
        image.preserveAspect = true;
        text.text = LocalizationManager.Instance.GetLocalizedString(data.text);
        data.text.StringChanged += value => text.text = value;
    }
}
