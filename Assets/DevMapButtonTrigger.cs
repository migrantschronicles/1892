using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMapButtonTrigger : MonoBehaviour
{
    public GameObject buttons;

    public void TriggerButtons()
    {
        //buttons.SetActive(!buttons.activeSelf);
        LevelInstance.Instance.ToggleDeveloperMenu();
    }
}
