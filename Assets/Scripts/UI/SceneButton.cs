using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    [SerializeField, Tooltip("The scene to open for this dialog")]
    private string sceneName;
    [SerializeField]
    private Button button;

    private void Awake()
    {
        if(string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.Log($"SceneButton {name} has no scene name set");
        }
    }

    private void Start()
    {
        button.onClick.AddListener(SwitchScenes);
    }

    private void SwitchScenes()
    {
        if(!string.IsNullOrWhiteSpace(sceneName))
        {
            LevelInstance.Instance.OpenScene(sceneName);
        }
    }
}
