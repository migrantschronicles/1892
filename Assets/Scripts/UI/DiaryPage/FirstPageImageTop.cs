using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FirstPageImageBottom : FirstPage
{
    [SerializeField]
    private Image image;

    public override IEnumerable<ElementAnimator> CreateAnimators()
    {
        return base.CreateAnimators().Concat(new List<ElementAnimator>()
        {
            ImageElementAnimator.FromImage(this, image)
        });
    }

    public override void SetData(DiaryEntryData entryData, DiaryPageData data)
    {
        base.SetData(entryData, data);
        image.sprite = data.image;
        image.preserveAspect = true;
    }
}
