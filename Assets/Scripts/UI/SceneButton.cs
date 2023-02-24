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
    [SerializeField]
    private Sprite sprite;
    [SerializeField]
    private Sprite pressedSprite;
    [SerializeField]
    private Sprite shopSprite;
    [SerializeField]
    private Sprite pressedShopSprite;
    [SerializeField]
    private bool isShopButton = false;

    private void Awake()
    {
        if(string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.Log($"SceneButton {name} has no scene name set");
        }
    }

    private void Start()
    {
        UpdateElements();
        button.onClick.AddListener(SwitchScenes);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateElements();
    }
#endif

    private void UpdateElements()
    {
        ((Image)button.targetGraphic).sprite = isShopButton ? shopSprite : sprite;
        SpriteState state = button.spriteState;
        state.pressedSprite = isShopButton ? pressedShopSprite : pressedSprite;
        state.highlightedSprite = isShopButton ? pressedShopSprite : pressedSprite;
        button.spriteState = state;
    }

    private void SwitchScenes()
    {
        if(!string.IsNullOrWhiteSpace(sceneName))
        {
            LevelInstance.Instance.OpenScene(sceneName);
        }
    }
}
