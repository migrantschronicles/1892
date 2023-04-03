using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemToShopAfterDays : MonoBehaviour
{
    [SerializeField]
    private int afterDays = 0;
    [SerializeField]
    private Item item;

    private bool added = false;

    private void Start()
    {
        if(item)
        {
            NewGameManager.Instance.onNewDay += OnNewDay;
            Check();
        }
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }

    private void OnNewDay()
    {
        Check();   
    }

    private void Check()
    {
        if (NewGameManager.Instance.DaysInCity == afterDays && item && !added)
        {
            GetComponent<Shop>().AddItem(item);
            added = true;
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }
}
