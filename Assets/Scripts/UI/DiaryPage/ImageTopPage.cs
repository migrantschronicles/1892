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

    private DiaryPageData pageData;

    private void OnDestroy()
    {
        if (pageData != null)
        {
            pageData.OnTextChanged -= OnTextChanged;
        }
    }

    public IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            ImageElementAnimator.FromImage(this, image),
            TextElementAnimator.FromText(this, text)
        };
    }

    public void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        pageData = data;
        image.sprite = data.image;
        image.preserveAspect = true;
        text.text = data.Text;
        data.OnTextChanged += OnTextChanged;
    }

    private void OnTextChanged(string value)
    {
        text.text = value;
    }
}
