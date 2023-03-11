using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndDayStopoverPopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Button stayButton;

    public bool CanClose { get { return NewGameManager.Instance.RemainingTime > 0; } }
    public InterfaceVisibilityFlags InterfaceVisibilityFlags { get { return InterfaceVisibilityFlags.ClockButton; } }
    public IPopup.OnCanCloseChangedEvent OnCanCloseChanged;

    private void Start()
    {
        stayButton.gameObject.SetActive(CanClose);
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
        if(!CanClose)
        {
            OnCanCloseChanged?.Invoke(this, false);
            stayButton.gameObject.SetActive(false);
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

    public void OnGoOnBoard()
    {
        LevelInstance.Instance.OnReturnFromStopover(true);
    }

    public void OnStayInCity()
    {
        LevelInstance.Instance.PopPopup();
    }
}
