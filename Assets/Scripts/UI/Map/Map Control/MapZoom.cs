using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapZoom : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    private float mouseZoomSpeed = 15.0f;
#endif
    [SerializeField]
    private float touchZoomSpeed = 0.1f;
    [SerializeField]
    private float minZoom = 0.1f;
    [SerializeField]
    private float maxZoom = 3.0f;
    [SerializeField]
    private float initialZoom = 1.0f;
    [SerializeField]
    private float initialZoomStart = 0.1f;
    [SerializeField]
    private float initialZoomTarget = 3.0f;
    [SerializeField]
    private AnimationCurve initialZoomCurve;
    [SerializeField]
    private float initialZoomDuration = 3.0f;
    [SerializeField, Tooltip("The maximum position the map can move in 1 second towards the current location marker.")]
    private float initialZoomMaxDelta = 100.0f;
    [SerializeField, Tooltip("At which distance to the current location marker the maximum position the map can move is decreased")]
    private float initialZoomMoveThreshold = 100.0f;

    private float zoomLevel = 1.0f;
    private Vector2 originalScale;
    private RectTransform rectTransform;
    private float initialZoomCurrentTime = -1.0f;

    public delegate void OnMapZoomChangedEvent(float zoomLevel);
    public event OnMapZoomChangedEvent onMapZoomChangedEvent;

    public float ZoomLevel
    {
        get
        {
            return zoomLevel;
        }
    }

    public float MinZoomLevel
    {
        get
        {
            return minZoom;
        }
    }
    public float MaxZoomLevel
    {
        get
        {
            return maxZoom;
        }
    }

    private bool IsInitialZoomInProgress
    {
        get
        {
            return initialZoomCurrentTime >= 0.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        rectTransform = GetComponent<RectTransform>();
        initialZoomTarget = Mathf.Clamp(initialZoomTarget, minZoom, maxZoom);
        initialZoomMaxDelta /= initialZoomDuration;
        SetZoom(Mathf.Clamp(initialZoom, minZoom, maxZoom), rectTransform.TransformPoint(rectTransform.rect.center));
    }

    // Update is called once per frame
    void Update()
    {
        if(IsInitialZoomInProgress)
        {
            initialZoomCurrentTime += Time.deltaTime;
            Vector2 zoomPoint = (Vector2) NewGameManager.Instance.currentLocationGO.transform.position;
            if (initialZoomCurrentTime < initialZoomDuration)
            {
                float alpha = initialZoomCurrentTime / initialZoomDuration;
                float currentValue = initialZoomCurve.Evaluate(alpha);
                float currentZoom = RemapValue(currentValue, 0, 1, initialZoomStart, initialZoomTarget);
                SetZoom(currentZoom, zoomPoint);

                // Move the map a bit so the current location gets into the center
                Vector2 currentPosition = (Vector2)NewGameManager.Instance.currentLocationGO.transform.position;
                RectTransform parentTransform = transform.parent.GetComponent<RectTransform>();
                Vector2 targetPosition = parentTransform.TransformPoint(parentTransform.rect.center);
                Debug.Log($"{currentPosition} || {targetPosition}");
                float maxDelta = initialZoomMaxDelta * Time.deltaTime;
                Vector2 newPosition = new Vector2(
                    Mathf.MoveTowards(currentPosition.x, targetPosition.x, 
                        maxDelta * Mathf.Clamp01(Mathf.Abs(targetPosition.x - currentPosition.x) / initialZoomMoveThreshold)),
                    Mathf.MoveTowards(currentPosition.y, targetPosition.y, 
                        maxDelta * Mathf.Clamp01(Mathf.Abs(targetPosition.y - currentPosition.y) / initialZoomMoveThreshold))
                );
                Vector2 deltaPosition = newPosition - currentPosition;
                rectTransform.anchoredPosition = rectTransform.anchoredPosition + deltaPosition;
            }
            else
            {
                SetZoom(initialZoomTarget, zoomPoint);
                initialZoomCurrentTime = -1.0f;
            }
        }
        else
        {
            // If this is an editor build, check if the mouse wheel was used (for testing).
#if UNITY_EDITOR
            float mouseScroll = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(mouseScroll, 0.0f))
            {
                Zoom(mouseScroll, mouseZoomSpeed, Input.mousePosition);
            }
            else
            {
#endif
                // In non-editor builds, only touch should be used.
                if (Input.touchCount == 2)
                {
                    // Get the current touch positions
                    Touch t0 = Input.GetTouch(0);
                    Touch t1 = Input.GetTouch(1);

                    // Get the previous touch positions.
                    Vector2 t0Prev = t0.position - t0.deltaPosition;
                    Vector2 t1Prev = t1.position - t1.deltaPosition;

                    // Get the delta distance
                    float prevTouchDistance = Vector2.Distance(t0Prev, t1Prev);
                    float touchDistance = Vector2.Distance(t0.position, t1.position);
                    float deltaDistance = prevTouchDistance - touchDistance;
                    Vector2 zoomPoint = (t0.position + t1.position) / 2;
                    Zoom(-deltaDistance, touchZoomSpeed, zoomPoint);
                }
#if UNITY_EDITOR
            }
#endif
        }
    }

    ///@todo Should be extension or static utility function
    public static float RemapValue(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private void Zoom(float delta, float speed, Vector2 zoomPoint)
    {
        // Set the new zoom level.
        float newZoomLevel = Mathf.Clamp(zoomLevel + delta * speed, minZoom, maxZoom);
        SetZoom(newZoomLevel, zoomPoint);
    }

    private void SetZoom(float zoom, Vector2 zoomPoint)
    {
        if(Mathf.Approximately(zoom, zoomLevel))
        {
            return;
        }

        zoomLevel = zoom;
        Vector3 newScale = originalScale * zoomLevel;

        // Calculate the new pivot to scale around.
        Vector2 size = rectTransform.rect.size;
        size.Scale(rectTransform.localScale);
        RectTransform parentRect = ((RectTransform)rectTransform.parent);
        Vector2 parentSize = parentRect.rect.size;
        parentSize.Scale(parentRect.localScale);
        if (size.x > parentSize.x && size.y > parentSize.y)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, zoomPoint, null, out Vector2 p2))
            {
                Vector2 pivotP = rectTransform.pivot * rectTransform.rect.size;
                Vector2 p3 = (Vector2)p2 + pivotP;
                Vector2 newPivot = p3 / rectTransform.rect.size;
                newPivot.x = Mathf.Clamp01(newPivot.x);
                newPivot.y = Mathf.Clamp01(newPivot.y);
                SetPivot(newPivot);
            }
        }
        else
        {
            SetPivot(new Vector2(0.5f, 0.5f));
        }

        // Set the new scale
        transform.localScale = newScale;

        // Broadcast
        if(onMapZoomChangedEvent != null)
        {
            onMapZoomChangedEvent.Invoke(zoomLevel);
        }
    }

    private void SetPivot(Vector2 pivot)
    {
        Vector3 deltaPosition = rectTransform.pivot - pivot;
        deltaPosition.Scale(rectTransform.rect.size);
        deltaPosition.Scale(rectTransform.localScale);
        deltaPosition = rectTransform.rotation * deltaPosition;

        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public void ResetInitialZoom()
    {
        initialZoomCurrentTime = 0.0f;
    }
}
