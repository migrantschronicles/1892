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
    [SerializeField]
    private Vector2 textWeight;

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
            TextElementAnimator.FromText(this, text),
            ImageElementAnimator.FromImage(this, image)
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

    public List<Vector2> GetTextFieldWeights()
    {
        return new List<Vector2> { textWeight };
    }
}
