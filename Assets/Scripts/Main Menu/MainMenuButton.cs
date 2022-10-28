using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    private Sprite enabledSprite;
    [SerializeField]
    private Sprite disabledSprite;

    private Button button;
    private Image image;

    public delegate void OnClickDelegate();
    public event OnClickDelegate onClick;

    private void Awake()
    {
        EnsureInitialized();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    void EnsureInitialized()
    {
        if(!button)
        {
            button = GetComponent<Button>();
        }

        if(!image)
        {
            image = GetComponent<Image>();
        }

        if(!enabledSprite)
        {
            enabledSprite = image.sprite;
        }
    }

    public void SetEnabled(bool enabled)
    {
        EnsureInitialized();
        button.enabled = enabled;
        image.sprite = enabled ? enabledSprite : disabledSprite;
        GetComponentInChildren<Text>().color = enabled ? Color.white : Color.black;
    }
}
