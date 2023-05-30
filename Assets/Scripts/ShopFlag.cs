using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopFlag : MonoBehaviour
{
    [SerializeField]
    private string sceneName;
    [SerializeField]
    private SpriteRenderer poleRenderer;
    [SerializeField]
    private SpriteRenderer flagRenderer;
    [SerializeField]
    private bool isLeft = true;
    [SerializeField]
    private Vector3 leftFlagPosition;
    [SerializeField]
    private Vector3 rightFlagPosition;
    [SerializeField]
    private Sprite leftPole;
    [SerializeField]
    private Sprite rightPole;
    [SerializeField]
    private Sprite defaultFlag;
    [SerializeField]
    private Sprite pressedFlag;
    [SerializeField]
    private float timeCountsAsClick = 0.3f;

    private float touchedTime = -1.0f;

    private void Start()
    {
        UpdateElements();
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

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchedTime = Time.time;
                flagRenderer.sprite = pressedFlag;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                touchedTime = -1.0f;
                flagRenderer.sprite = defaultFlag;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (touchedTime >= 0.0f)
                {
                    // Else the touch was moved
                    float delta = Time.time - touchedTime;
                    if (delta <= timeCountsAsClick)
                    {
                        OnClick(touch.position);
                    }

                    touchedTime = -1.0f;
                    flagRenderer.sprite = defaultFlag;
                }
            }
        }
        else
        {
            touchedTime = -1.0f;
            flagRenderer.sprite = defaultFlag;
        }
    }

    private void UpdateElements()
    {
        poleRenderer.sprite = isLeft ? leftPole : rightPole;
        flagRenderer.transform.localPosition = isLeft ? leftFlagPosition : rightFlagPosition;
    }

    private void OnMouseDown()
    {
        flagRenderer.sprite = pressedFlag;
    }

    private void OnMouseEnter()
    {
        flagRenderer.sprite = pressedFlag;
    }

    private void OnMouseExit()
    {
        flagRenderer.sprite = defaultFlag;
    }

    private void OnMouseUp()
    {
        flagRenderer.sprite = defaultFlag;
    }

    private void OnMouseUpAsButton()
    {
        flagRenderer.sprite = defaultFlag;
        GoToTarget();
    }

    private void OnClick(Vector2 screenPosition)
    {
        if (ShipRoomNavigation.SpriteCast(screenPosition).collider != null)
        {
            GoToTarget();
        }
    }

    private void GoToTarget()
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            LevelInstance.Instance.OpenScene(sceneName);
        }
    }
}
