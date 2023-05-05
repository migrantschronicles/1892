using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private AnimationCurve autoZoomLocationCurve;
    [SerializeField]
    private float initialZoomDuration = 3.0f;
    [SerializeField]
    private Button centerButton;
    [SerializeField]
    private float level1Breakpoint = 0.5f;
    [SerializeField]
    private float level2Breakpoint = 1.5f;
    [SerializeField]
    private Vector3 mapScreenshotPosition = new Vector3(711, 418);
    [SerializeField]
    private Vector3 mapScreenshotScale = new Vector3(0.8f, 0.8f);
    [SerializeField]
    private Vector2 mapScreenshotPivot = new Vector2(0.5f, 0.5f);
    [SerializeField]
    private Vector2 normalizedCenterAdjustment = new Vector2(-7.0f, 0.0f);

    private float zoomLevel = 1.0f;
    private Vector2 originalScale;
    private RectTransform rectTransform;
    private float autoZoomCurrentTime = -1.0f;
    private float autoZoomStart = 1.0f;
    private Vector2 autoZoomNormalizedStartPosition;
    private ScrollRect scrollRect;
    private Vector3 oldPivot;
    private Vector3 oldLocalPosition;
    private Vector3 oldLocalScale;
    private Map map;

    public delegate void OnMapZoomChangedEvent(float zoomLevel);
    public event OnMapZoomChangedEvent onMapZoomChangedEvent;

    public float Level1Breakpoint { get { return level1Breakpoint; } }
    public float Level2Breakpoint { get { return level2Breakpoint; } }
    public int VisibleLevel
    {
        get
        {
            if(zoomLevel >= level2Breakpoint)
            {
                return 2;
            }
            else if(zoomLevel >= Level1Breakpoint)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public float ZoomLevel { get { return zoomLevel; } }
    public float MinZoomLevel { get { return minZoom; } }
    public float MaxZoomLevel { get { return maxZoom; } }
    private bool IsAutoZoomInProgress { get { return autoZoomCurrentTime >= 0.0f; } }
    public float DefaultZoomLevel { get { return initialZoom; } }

    private void Awake()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        rectTransform = GetComponent<RectTransform>();
        map = GetComponent<Map>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        initialZoomTarget = Mathf.Clamp(initialZoomTarget, minZoom, maxZoom);
        SetZoom(Mathf.Clamp(initialZoom, minZoom, maxZoom), rectTransform.TransformPoint(rectTransform.rect.center));
        centerButton.onClick.AddListener(OnCenterMap);
        ResetInitialZoom();
        LevelInstance.Instance.IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAutoZoomInProgress)
        {
            autoZoomCurrentTime += Time.deltaTime;
            GameObject currentFocusObject = map.CurrentFocusObject;
            if (currentFocusObject)
            {
                Vector2 targetMarkerPosition = currentFocusObject.GetComponent<RectTransform>().anchoredPosition;
                if (autoZoomCurrentTime < initialZoomDuration)
                {
                    float alpha = autoZoomCurrentTime / initialZoomDuration;
                    float currentZoomValue = initialZoomCurve.Evaluate(alpha);
                    float currentZoom = RemapValue(currentZoomValue, 0, 1, autoZoomStart, initialZoomTarget);
                    SetZoomAtCenter(currentZoom);

                    float currentLocationValue = autoZoomLocationCurve.Evaluate(alpha);
                    Vector2 currentCenter = Vector2.Lerp(autoZoomNormalizedStartPosition, targetMarkerPosition, currentLocationValue);
                    SetCenter(currentCenter);
                }
                else
                {
                    autoZoomCurrentTime = -1.0f;
                    SetCenterToMarker(currentFocusObject);
                }
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
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, zoomPoint, Camera.main, out Vector2 p2))
            {
                Vector2 pivotP = rectTransform.pivot * rectTransform.rect.size;
                Vector2 p3 = p2 + pivotP;
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
        // GetComponentInParent does not work because it does not include inactive game objects
        DiaryContentPage page = GetComponentsInParent<DiaryContentPage>(true)[0];
        if(page.Status == OpenStatus.Opened)
        {
            OnPageStatusChanged(OpenStatus.Opened);
        }
        else
        {
            page.onStatusChanged += OnPageStatusChanged;
            SetZoomAtCenter(autoZoomStart);
        }
    }

    private void OnPageStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Opened)
        {
            DiaryContentPage page = GetComponentInParent<DiaryContentPage>();
            page.onStatusChanged -= OnPageStatusChanged;
            autoZoomCurrentTime = 0.0f;
            autoZoomStart = initialZoomStart;
        }
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        if(status == OpenStatus.Opening)
        {
            ResetInitialZoom();
        }
    }

    private void OnCenterMap()
    {
        autoZoomCurrentTime = 0.0f;
        autoZoomStart = zoomLevel;
        autoZoomNormalizedStartPosition = GetCenter();
    }

    private void SetZoomAtCenter(float newZoomLevel)
    {
        if (Mathf.Approximately(newZoomLevel, zoomLevel))
        {
            return;
        }

        zoomLevel = newZoomLevel;
        Vector3 newScale = originalScale * zoomLevel;

        Vector2 centerPosition = GetCenter();
        Vector2 normalizedCenterPosition = centerPosition / rectTransform.rect.size;
        SetPivot(normalizedCenterPosition + new Vector2(0.5f, 0.5f));

        // Set the new scale
        transform.localScale = newScale;

        // Broadcast
        if (onMapZoomChangedEvent != null)
        {
            onMapZoomChangedEvent.Invoke(zoomLevel);
        }
    }

    /**
     * Sets the center to this position.
     * This could be a location marker.
     * If sizes haven't changed the map content is 1744/1319.
     * This is the position from the center of that content, i.e. range from -872/-659.5 to 872/659.5
     */
    private void SetCenter(Vector2 normalizedPosition)
    {
        // The viewport size, in the map content size transform
        Vector2 normalizedViewportSize = transform.parent.GetComponent<RectTransform>().rect.size / rectTransform.localScale;
        // The size minus how much the viewport can go.
        Vector2 boundedSize = rectTransform.rect.size - normalizedViewportSize;
        // Set the normalized position
        scrollRect.normalizedPosition = (normalizedPosition + normalizedCenterAdjustment) / boundedSize + new Vector2(0.5f, 0.5f);
    }

    private void SetCenterToMarker(GameObject marker)
    {
        SetCenter(marker.GetComponent<RectTransform>().anchoredPosition);
    }

    private Vector2 GetCenter()
    {
        // The viewport size, in the map content size transform
        Vector2 normalizedViewportSize = transform.parent.GetComponent<RectTransform>().rect.size / rectTransform.localScale;
        // The size minus how much the viewport can go.
        Vector2 boundedSize = rectTransform.rect.size - normalizedViewportSize;

        return (scrollRect.normalizedPosition - new Vector2(0.5f, 0.5f)) * boundedSize;
    }

    public bool IsVisible(Rect rect)
    {
        // The viewport size, in the map content size transform
        Vector2 normalizedViewportSize = transform.parent.GetComponent<RectTransform>().rect.size / rectTransform.localScale;

        Vector2 center = GetCenter();
        center += rectTransform.rect.size / 2;

        Rect viewportRect = new Rect(center - normalizedViewportSize / 2, normalizedViewportSize);
        return viewportRect.Overlaps(rect);
    }
}
