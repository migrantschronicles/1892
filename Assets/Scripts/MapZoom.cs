using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // If this is an editor build, check if the mouse wheel was used (for testing).
#if UNITY_EDITOR
        float mouseScroll = Input.mouseScrollDelta.y;
        if (!Mathf.Approximately(mouseScroll, 0.0f))
        {
            Zoom(mouseScroll, mouseZoomSpeed);
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
                Zoom(-deltaDistance, touchZoomSpeed);
            }
#if UNITY_EDITOR
        }
#endif
    }

    void Zoom(float delta, float speed)
    {
        zoomLevel = Mathf.Clamp(zoomLevel + delta * speed, minZoom, maxZoom);
        transform.localScale = originalScale * zoomLevel;
    }
}
