using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class VisitCityPopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Text visitCityText;
    [SerializeField]
    private LocalizedString visitCityString;
    
    public event IPopup.OnPopupAction OnVisit;
    public event IPopup.OnPopupAction OnStayOnBoard;

    public bool CanClose { get { return false; } }

    public void OnVisitClicked()
    {
        OnVisit?.Invoke(this);
    }

    public void OnStayOnBoardClicked()
    {
        OnStayOnBoard?.Invoke(this);
    }

    public void SetDestinationCity(string location)
    {
        string localizedName = NewGameManager.Instance.LocationManager.GetLocalizedName(location);
        visitCityText.text = LocalizationManager.Instance.GetLocalizedString(visitCityString, localizedName);
    }
}
