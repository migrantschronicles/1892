using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class TransportationManager : MonoBehaviour
{
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
    [SerializeField]
    private LocalizedString ferryString;

    public string GetLocalizedMethod(TransportationMethod method, bool useFerry = false)
    {
        LocalizedString transportationTitle = null;
        switch (method)
        {
            case TransportationMethod.Walking: transportationTitle = walkingString; break;
            case TransportationMethod.Tram: transportationTitle = tramString; break;
            case TransportationMethod.Train: transportationTitle = trainString; break;
            case TransportationMethod.Cart: transportationTitle = cartString; break;
            case TransportationMethod.Ship: transportationTitle = useFerry ? ferryString : shipString; break;
            case TransportationMethod.Carriage: transportationTitle = carriageString; break;
            default: return "None";
        }

        return LocalizationManager.Instance.GetLocalizedString(transportationTitle);
    }
}
