using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapZoom : MonoBehaviour
{
    [SerializeField]
    float mouseZoomSpeed = 15.0f;
    [SerializeField]
    float touchZoomSpeed = 0.1f;
    [SerializeField]
    float minZoom = 0.1f;
    [SerializeField]
    float maxZoom = 3.0f;

    private float zoomLevel = 1.0f;
    private Vector2 originalScale;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
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
            if(Input.touchCount == 2)
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

    void Zoom(float delta, float speed, Vector2 zoomPoint)
    {
        // Set the new zoom level.
        float oldZoomLevel = zoomLevel;
        zoomLevel = Mathf.Clamp(zoomLevel + delta * speed, minZoom, maxZoom);
        if(Mathf.Approximately(zoomLevel, oldZoomLevel))
        {
            return;
        }

        Vector3 newScale = originalScale * zoomLevel;

        // Calculate the new pivot to scale around.
        Vector2 size = rectTransform.rect.size;
        size.Scale(rectTransform.localScale);
        RectTransform parentRect = ((RectTransform)rectTransform.parent);
        Vector2 parentSize = parentRect.rect.size;
        parentSize.Scale(parentRect.localScale);
        if (size.x > parentSize.x && size.y > parentSize.y)
        {
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, zoomPoint, null, out Vector2 p2))
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
}
