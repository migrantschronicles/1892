using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullTextPage : MonoBehaviour, IDiaryPage
{
    [SerializeField]
    private Text text;

    public virtual IEnumerable<ElementAnimator> CreateAnimators()
    {
        return new List<ElementAnimator>()
        {
            TextElementAnimator.FromText(this, text)
        };
    }

    public virtual void SetData(DiaryPageData data)
    {
        text.text = LocalizationManager.Instance.GetLocalizedString(data.text);
        data.text.StringChanged += value => text.text = value;
    }
}
