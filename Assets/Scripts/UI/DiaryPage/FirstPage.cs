using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Text dateText;
    [SerializeField]
    private Text text;

    protected DiaryEntryData entryData;
    protected DiaryPageData pageData;

    public virtual IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            TextElementAnimator.FromText(this, dateText),
            TextElementAnimator.FromText(this, text)
        };
    }

    private void OnDestroy()
    {
        if(entryData != null)
        {
            entryData.OnDateChanged -= OnDateChanged;
        }

        if(pageData != null)
        {
            pageData.OnTextChanged -= OnTextChanged;
        }
    }

    public virtual void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        this.entryData = entryData;
        this.pageData = data;
        text.text = data.Text;
        data.OnTextChanged += OnTextChanged;
        dateText.text = entryData.LocalizedDate;
        entryData.OnDateChanged += OnDateChanged;
    }

    private void OnTextChanged(string value)
    {
        text.text = value;
    }

    private void OnDateChanged(string date)
    {
        dateText.text = date;
    }
}
