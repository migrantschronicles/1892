using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShipRoomNavigation : MonoBehaviour
{
    [SerializeField]
    private float timeCountsAsClick = 0.3f;

    private float touchedTime = -1.0f;
    private Vector3 lastPosition;
    private ShipMovement shipMovement;

    private void Start()
    {
        shipMovement = GetComponent<ShipMovement>();
    }

    private void Update()
    {
        if (!shipMovement.IsAutoZooming)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (Input.GetMouseButtonDown(0))
            {
                touchedTime = Time.time;
                lastPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                float delta = Time.time - touchedTime;
                if (delta <= timeCountsAsClick && (Input.mousePosition - lastPosition).magnitude < 1)
                {
                    OnClick(Input.mousePosition);
                }

                touchedTime = -1.0f;
            }
#else
            if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if(touch.phase == TouchPhase.Began)
                {
                    touchedTime = Time.time;
                }
                else if(touch.phase == TouchPhase.Moved)
                {
                    touchedTime = -1.0f;
                }
                else if(touch.phase == TouchPhase.Ended)
                {
                    if(touchedTime >= 0.0f)
                    {
                        // Else the touch was moved
                        float delta = Time.time - touchedTime;
                        if(delta <= timeCountsAsClick)
                        {
                            OnClick(touch.position);
                        }

                        touchedTime = -1.0f;
                    }
                }
            }
            else
            {
                touchedTime = -1.0f;
            }
#endif
        }
    }

    private bool IsOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void OnClick(Vector2 screenPosition)
    {
        if(IsOverUI())
        {
            return;
        }

        Vector3 worldPosition = LevelInstance.Instance.MainCamera.ScreenToWorldPoint(screenPosition);
        Vector2 ray = new Vector2(worldPosition.x, worldPosition.y);
        RaycastHit2D hit = Physics2D.Raycast(ray, ray, 0.1f);
        if(hit.collider != null)
        {
            Room room = hit.collider.GetComponentInParent<Room>();
            if(LevelInstance.Instance.GoToRoom(room))
            {
                LevelInstance.Instance.GetComponent<ShipMovement>().ZoomToTarget(room.transform.position);
            }
        }
    }
}
