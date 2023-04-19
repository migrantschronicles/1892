using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullTextPage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Text text;

    private DiaryPageData pageData;

    public virtual IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            TextElementAnimator.FromText(this, text)
        };
    }

    private void OnDestroy()
    {
        if(pageData != null)
        {
            pageData.OnTextChanged -= OnTextChanged;
        }
    }

    public virtual void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        pageData = data;
        text.text = data.Text;
        data.OnTextChanged += OnTextChanged;
    }

    private void OnTextChanged(string value)
    {
        text.text = value;
    }
}
