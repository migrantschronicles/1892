using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DiscoveredRoutePopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Text title;

    public event IPopup.OnPopupAction OnAccepted;

    public void Accept()
    {
        OnAccepted?.Invoke(this);
    }

    public void Init(string destination, IEnumerable<TransportationMethod> methods)
    {
        string methodsString = methods.Select(method => method.ToString()).Aggregate((a, b) => $"{a}, {b}");
        LocalizeStringEvent localizeStringEvent = title.GetComponent<LocalizeStringEvent>();
        localizeStringEvent.StringReference.Arguments = new object[] { destination, methodsString };
        localizeStringEvent.RefreshString();
    }
}
