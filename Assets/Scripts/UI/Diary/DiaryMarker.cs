using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiaryMarker : MonoBehaviour
{
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite activeSprite;
    [SerializeField]
    private Sprite closedSprite;

    private bool closed = false;
    private bool active = false;
    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        UpdateSprite();
    }

    public void SetActive(bool active)
    {
        this.active = active;
        UpdateSprite();
    }

    public void SetClosed(bool closed)
    {
        this.closed = closed;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Sprite sprite = closed ? closedSprite : (active ? activeSprite : defaultSprite);
        image.sprite = sprite;
        rectTransform.sizeDelta = sprite.rect.size;
    }
}
