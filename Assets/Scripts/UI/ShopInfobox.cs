using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ShopInfobox : MonoBehaviour
{
    [SerializeField]
    private LocalizedString shopText;
    [SerializeField]
    private LocalizedString freeShopText;
    [SerializeField]
    private Text text;

    public void SetIsShop(bool isShop)
    {
        LocalizedString s = isShop ? shopText : freeShopText;
        OnStringChanged(LocalizationManager.Instance.GetLocalizedString(s));
    }

    private void OnStringChanged(string value)
    {
        text.text = value;
    }
}
