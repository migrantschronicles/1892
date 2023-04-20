using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TransportationMethodBox : MonoBehaviour
{
    [SerializeField]
    private TransportationMethod method;
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Image handle;
    [SerializeField]
    private bool useFerry = false;

    public TransportationRouteInfo RouteInfo
    {
        set
        {
            moneyText.text = value.cost.ToString();
        }
    }

    private void Start()
    {
        UpdateTransportationMethod();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if(this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateTransportationMethod();
    }
#endif

    private void UpdateTransportationMethod()
    {
        Vector2 normalizedPosition = Vector2.zero;
        switch(method)
        {
            case TransportationMethod.Walking: normalizedPosition.Set(0, -1); break;
            case TransportationMethod.Tram: normalizedPosition.Set(-1, -1); break;
            case TransportationMethod.Train: normalizedPosition.Set(0, 1); break;
            case TransportationMethod.Cart: normalizedPosition.Set(1, -1); break;
            case TransportationMethod.Ship: normalizedPosition.Set(1, 1); break;
            case TransportationMethod.Carriage: normalizedPosition.Set(-1, 1); break;
            default: titleText.text = "NONE"; return;
        }

        if(Application.isPlaying)
        {
            titleText.text = NewGameManager.Instance.TransportationManager.GetLocalizedMethod(method, useFerry);
        }
        else
        {
            titleText.text = method.ToString();
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform handleTransform = handle.GetComponent<RectTransform>();
        Vector2 newPosition = rectTransform.sizeDelta / 2 * normalizedPosition;
        handleTransform.anchoredPosition = newPosition;
    }

    public void SetUseFerry(bool ferry)
    {
        useFerry = ferry;
        UpdateTransportationMethod();
    }
}
