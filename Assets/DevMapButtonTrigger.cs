using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevMapButtonTrigger : MonoBehaviour
{
    public void TriggerButtons()
    {
        LevelInstance.Instance.ToggleDeveloperMenu();
    }
}
