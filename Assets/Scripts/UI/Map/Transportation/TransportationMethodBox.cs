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
    private Text timeText;
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Image handle;
    [SerializeField]
    private LocalizedString walkingString;
    [SerializeField]
    private LocalizedString tramString;
    [SerializeField]
    private LocalizedString trainString;
    [SerializeField]
    private LocalizedString cartString;
    [SerializeField]
    private LocalizedString shipString;
    [SerializeField]
    private LocalizedString carriageString;

    public TransportationRouteInfo RouteInfo
    {
        set
        {
            timeText.text = $"{(int)(value.time / 86400)}d {(int)((value.time % 86400) / 3600)}h {(int)((value.time % 3600) / 60)}m";
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
        LocalizedString transportationTitle = null;
        Vector2 normalizedPosition = Vector2.zero;
        switch(method)
        {
            case TransportationMethod.Walking: transportationTitle = walkingString; normalizedPosition.Set(0, -1); break;
            case TransportationMethod.Tram: transportationTitle = tramString; normalizedPosition.Set(-1, -1); break;
            case TransportationMethod.Train: transportationTitle = trainString; normalizedPosition.Set(0, 1); break;
            case TransportationMethod.Cart: transportationTitle = cartString; normalizedPosition.Set(1, -1); break;
            case TransportationMethod.Ship: transportationTitle = shipString; normalizedPosition.Set(1, 1); break;
            case TransportationMethod.Carriage: transportationTitle = carriageString; normalizedPosition.Set(-1, 1); break;
            default: titleText.text = "NONE"; return;
        }

        if(Application.isPlaying)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedString(transportationTitle);
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
}
