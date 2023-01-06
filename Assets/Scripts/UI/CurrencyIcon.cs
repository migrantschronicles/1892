using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyIcon : MonoBehaviour
{
    [SerializeField]
    private Sprite francDarkIcon;
    [SerializeField]
    private Sprite frankLightIcon;
    [SerializeField]
    private Sprite dollarDarkIcon;
    [SerializeField]
    private Sprite dollarLightIcon;
    [SerializeField]
    private bool useLight = false;

    private Image image;
    
    public bool UseLight { get { return useLight; } set { useLight = value; OnCurrencyChanged(NewGameManager.Instance.CurrentCurrency); } }

    private void Start()
    {
        image = GetComponent<Image>();
        NewGameManager.Instance.onCurrencyChanged += OnCurrencyChanged;
        OnCurrencyChanged(NewGameManager.Instance.CurrentCurrency);
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance != null)
        {
            NewGameManager.Instance.onCurrencyChanged -= OnCurrencyChanged;
        }
    }

    private void OnCurrencyChanged(Currency currency)
    {
        switch(currency)
        {
            case Currency.Franc: image.sprite = useLight ? frankLightIcon : francDarkIcon; break;
            case Currency.Dollar: image.sprite = useLight ? dollarLightIcon : dollarDarkIcon; break;
        }
    }
}
