using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageTopPage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text text;

    public IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            ImageElementAnimator.FromImage(this, image),
            TextElementAnimator.FromText(this, text)
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
