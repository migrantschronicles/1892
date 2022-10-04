using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPageSignatureImage : FirstPage
{
    [SerializeField]
    private Text signature;
    [SerializeField]
    private Image image;

    public override void SetData(DiaryPageData data)
    {
        base.SetData(data);
        signature.text = LocalizationManager.Instance.GetLocalizedString(data.text2);
        data.text2.StringChanged += value => signature.text = value;
        image.sprite = data.image;
        image.preserveAspect = true;
    }
}
