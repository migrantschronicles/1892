using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndDayStopoverPopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Button stayButton;

    public bool CanClose { get { return true; } }
    public InterfaceVisibilityFlags InterfaceVisibilityFlags { get { return InterfaceVisibilityFlags.ClockButton; } }

    private void Start()
    {
        stayButton.gameObject.SetActive(CanClose);
    }

    public void OnGoOnBoard()
    {
        LevelInstance.Instance.OnReturnFromStopover(true);
    }

    public void OnStayInCity()
    {
        LevelInstance.Instance.PopPopup();
    }
}
