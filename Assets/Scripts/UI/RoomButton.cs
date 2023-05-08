using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Text roomNameText;
    [SerializeField]
    private Color defaultForegroundColor = Color.black;
    [SerializeField]
    private Color highlightedForegroundColor = Color.white;

    private LocalizedString roomName;
    private Room room;
    private bool pointerInside = false;
    private bool pointerDown = false;

    public Room Room
    {
        get
        {
            return room;
        }
        set
        {
            room = value;

            if (roomName != null)
            {
                roomName.StringChanged -= OnRoomNameChanged;
            }

            roomName = room.RoomName;
            if (roomName != null)
            {
                roomName.StringChanged += OnRoomNameChanged;
                OnRoomNameChanged(LocalizationManager.Instance.GetLocalizedString(roomName));
            }

            PositionOnSprite positionOnSprite = GetComponent<PositionOnSprite>();
            positionOnSprite.WorldObject = room.gameObject;
            positionOnSprite.WorldObjectOffsetLocal = room.RoomButtonWorldObjectOffset;
        }
    }

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        LevelInstance.Instance.GoToRoom(Room);
        LevelInstance.Instance.GetComponent<ShipMovement>().ZoomToTarget(Room.transform.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        UpdateHighlighted();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        UpdateHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        UpdateHighlighted();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        UpdateHighlighted();
    }

    private void OnRoomNameChanged(string value)
    {
        roomNameText.text = value;
    }

    private void UpdateHighlighted()
    {
        SetHighlighted(pointerInside || pointerDown);
    }

    private void SetHighlighted(bool value)
    {
        roomNameText.color = value ? highlightedForegroundColor : defaultForegroundColor;
    }

    public void SetActive(bool active)
    {
        pointerDown = false;
        pointerInside = false;
        gameObject.SetActive(active);
        UpdateHighlighted();
    }
}
