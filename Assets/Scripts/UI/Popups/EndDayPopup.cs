using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class EndDayPopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Button hostelButton;
    [SerializeField]
    private LocalizeStringEvent descriptionLocalizeEvent;
    [SerializeField]
    private GameObject endDayHostelPrefab;
    [SerializeField]
    private GameObject endDayOutsidePrefab;

    public bool CanClose { get { return true; } }
    public InterfaceVisibilityFlags InterfaceVisibilityFlags { get { return InterfaceVisibilityFlags.ClockButton; } }

    private void Start()
    {
        // Disable hostel button if the player does not have enough money for the fee.
        EndDayHostelPopup hostelPopup = endDayHostelPrefab.GetComponent<EndDayHostelPopup>();
        if(NewGameManager.Instance.money < hostelPopup.HostelFee)
        {
            hostelButton.gameObject.SetActive(false);
        }

        descriptionLocalizeEvent.StringReference.Arguments = new List<object> { hostelPopup.HostelFee.ToString() };
        descriptionLocalizeEvent.RefreshString();
    }

    public void OnHostel()
    {
        LevelInstance.Instance.PushPopup(endDayHostelPrefab);
        TutorialManager.Instance.Blur.EndOfDay_Hostel();
    }

    public void OnOutside()
    {
        LevelInstance.Instance.PushPopup(endDayOutsidePrefab);
        TutorialManager.Instance.Blur.EndOfDay_Outside();
    }
}
