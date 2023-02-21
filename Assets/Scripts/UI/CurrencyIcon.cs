using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyIcon : MonoBehaviour
{
    [SerializeField]
    private Image image;
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

    public bool UseLight { get { return useLight; } set { useLight = value; OnCurrencyChanged(NewGameManager.Instance.CurrentCurrency); } }

    private void Start()
    {
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        OnCurrencyChanged(Currency.Franc);
    }
#endif

    private void OnCurrencyChanged(Currency currency)
    {
        switch(currency)
        {
            case Currency.Franc: image.sprite = useLight ? frankLightIcon : francDarkIcon; break;
            case Currency.Dollar: image.sprite = useLight ? dollarLightIcon : dollarDarkIcon; break;
        }
    }
}
