using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DiaryMarkerState
{
    Default,
    Closed,
    Active
}

public class DiaryMarker : MonoBehaviour
{
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite activeSprite;
    [SerializeField]
    private Sprite closedSprite;
    [SerializeField]
    private Vector2 defaultSize = new Vector2(69, 67);
    [SerializeField]
    private Vector2 activeSize = new Vector2(69, 88);

    private bool closed = false;
    private bool active = false;
    private Image image;
    private RectTransform rectTransform;

    public DiaryMarkerState State
    {
        get
        {
            return closed ? DiaryMarkerState.Closed : (active ? DiaryMarkerState.Active : DiaryMarkerState.Default);
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
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
        rectTransform.sizeDelta = active ? activeSize : defaultSize;
        if (!NewGameManager.Instance || NewGameManager.Instance.wantsEndGame)
        {
            GetComponent<Button>().interactable = false;
        }
    }
}
