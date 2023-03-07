using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnFromStopoverPopup : MonoBehaviour, IPopup
{
    [SerializeField]
    private Button stayInCityButton;

    public bool CanClose { get { return false; } }
    public bool CanStayInCity { get { return NewGameManager.Instance.RemainingTime > 0; } }

    private void Start()
    {
        stayInCityButton.gameObject.SetActive(CanStayInCity);
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
        if(!CanStayInCity)
        {
            stayInCityButton.gameObject.SetActive(false);
        }
    }

    public void OnBoardClicked()
    {
        LevelInstance.Instance.OnReturnFromStopover();
    }

    public void OnStayInCityClicked()
    {
        LevelInstance.Instance.PopPopup();
    }
}
