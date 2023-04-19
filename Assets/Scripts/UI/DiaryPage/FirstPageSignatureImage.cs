using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FirstPageSignatureImage : FirstPage
{
    [SerializeField]
    private Text signature;
    [SerializeField]
    private Image image;

    public override IEnumerable<ElementAnimator> CreateAnimators()
    {
        return base.CreateAnimators().Concat(new List<ElementAnimator>()
        {
            TextElementAnimator.FromText(this, signature),
            ImageElementAnimator.FromImage(this, image)
        });
    }

    public override void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        base.SetData(entryData, data);
        signature.text = data.Text2;
        data.OnText2Changed += OnText2Changed;
        image.sprite = data.image;
        image.preserveAspect = true;
    }

    private void OnText2Changed(string value)
    {
        signature.text = value;
    }
}
