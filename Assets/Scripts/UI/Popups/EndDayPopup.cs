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

    public bool CanClose { get { return NewGameManager.Instance.RemainingTime > 0; } }
    public IPopup.OnCanCloseChangedEvent OnCanCloseChanged;

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

    private void OnEnable()
    {
        NewGameManager.Instance.onTimeChanged += OnTimeChanged;
        OnTimeChanged(NewGameManager.Instance.hour, NewGameManager.Instance.minutes);
    }

    private void OnDisable()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onTimeChanged -= OnTimeChanged;
        }
    }

    private void OnTimeChanged(int hour, int minute)
    {
        if (!CanClose)
        {
            OnCanCloseChanged?.Invoke(this, false);
        }
    }

    public void AddOnCanCloseChangedListener(IPopup.OnCanCloseChangedEvent onCanCloseChanged) 
    {
        OnCanCloseChanged += onCanCloseChanged;
    }
    
    public void RemoveOnCanCloseChangedListener(IPopup.OnCanCloseChangedEvent onCanCloseChanged) 
    { 
        OnCanCloseChanged -= onCanCloseChanged;
    }

    public void OnHostel()
    {
        LevelInstance.Instance.PushPopup(endDayHostelPrefab);
    }

    public void OnOutside()
    {
        LevelInstance.Instance.PushPopup(endDayOutsidePrefab);
    }
}
