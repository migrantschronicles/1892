using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DiscoveredRoutePopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Text title;
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

    public event IPopup.OnPopupAction OnAccepted;

    public bool CanClose { get { return false; } }

    public void Accept()
    {
        OnAccepted?.Invoke(this);
    }

    private string GetLocalizedStringForMethod(TransportationMethod method)
    {
        LocalizedString localizedString = null;
        switch (method)
        {
            case TransportationMethod.Walking: localizedString = walkingString; break;
            case TransportationMethod.Tram: localizedString = tramString; break;
            case TransportationMethod.Train: localizedString = trainString; break;
            case TransportationMethod.Cart: localizedString = cartString; break;
            case TransportationMethod.Ship: localizedString = shipString; break;
            case TransportationMethod.Carriage: localizedString = carriageString; break;
        }

        return LocalizationManager.Instance.GetLocalizedString(localizedString);
    }

    public void Init(string destination, IEnumerable<TransportationMethod> methods)
    {
        string methodsString = methods.Select(method => GetLocalizedStringForMethod(method)).Aggregate((a, b) => $"{a}, {b}");
        string localizedLocation = NewGameManager.Instance.LocationManager.GetLocalizedName(destination);
        LocalizeStringEvent localizeStringEvent = title.GetComponent<LocalizeStringEvent>();
        localizeStringEvent.StringReference.Arguments = new object[] { localizedLocation, methodsString };
        localizeStringEvent.RefreshString();
    }
}
