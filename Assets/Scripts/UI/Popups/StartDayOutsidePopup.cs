using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class StartDayOutsidePopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Text text;
    [SerializeField]
    private Text stolenItemsText;
    [SerializeField]
    private LocalizedString itemsStolenText;
    [SerializeField]
    private LocalizedString noItemsStolenText;

    public event IPopup.OnPopupAction OnStartDay;
    public bool CanClose { get { return false; } }

    public void Init(List<StolenItemInfo> stolenItems)
    {
        if(stolenItems.Count == 0)
        {
            text.text = LocalizationManager.Instance.GetLocalizedString(noItemsStolenText);
            stolenItemsText.gameObject.SetActive(false);
        }
        else
        {
            string str = stolenItems.Select(item =>
            {
                if (item.type == StolenItemType.Money)
                {
                    return $"{item.money} {NewGameManager.Instance.CurrentCurrency}";
                }
                else
                {
                    return LocalizationManager.Instance.GetLocalizedString(item.item.Name);
                }
            }).Aggregate((a, b) => $"{a}, {b}");

            text.text = LocalizationManager.Instance.GetLocalizedString(itemsStolenText);
            stolenItemsText.text = str;
        }
    }

    public void OnAccept()
    {
        OnStartDay?.Invoke(this);
    }
}
