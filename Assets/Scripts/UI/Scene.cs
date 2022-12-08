using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    [SerializeField]
    private GameObject interactables;
    [SerializeField]
    private string sceneName;

    public string SceneName { get { return sceneName; } }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is empty in: " + name);
        }
    }

    public void OnActiveStatusChanged(bool active)
    {
        if(active && interactables)
        {
            interactables.SetActive(true);
        }
    }

    public void SetInteractablesVisible(bool visible)
    {
        if(interactables)
        {
            interactables.SetActive(visible);
        }
    }
}
