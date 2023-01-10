using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    [SerializeField]
    private Rect cameraBounds = new Rect(-80, -30, 160, 60);
    [SerializeField]
    private Vector2 zoomBounds = new Vector2(0.1f, 3.0f);
    [SerializeField]
    private float moveMultiplier = 1.0f;
    [SerializeField]
    private float zoomMultiplier = 1.0f;

    private float originalZ = -1.0f;
    private float zoomValue = 1.0f;

    public delegate void OnZoomValueChangedEvent(float zoomValue);
    public event OnZoomValueChangedEvent onZoomValueChanged;

    public Vector2 ZoomBounds { get { return zoomBounds; } }
    public float ZoomValue { get { return zoomValue; } }

#if UNITY_EDITOR
    [SerializeField]
    private float mouseSpeedMultiplier = 0.1f;
    [SerializeField]
    private float mouseZoomMultiplier = 0.1f;

    private Vector2 lastMousePosition = Vector2.zero;
#endif

    private void Start()
    {
        originalZ = LevelInstance.Instance.MainCamera.orthographicSize;
    }

    private void Update()
    {
        if(Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                // If there is only one touch, move the camera.
                Touch t0 = Input.GetTouch(0);
                // Normalize it to viewport 0..1
                Vector2 delta = NormalizeTouchPoint(t0.deltaPosition);
                // Scale with zoom level.
                delta *= Mathf.Abs(LevelInstance.Instance.MainCamera.orthographicSize);
                // Scale differently in x / y
                delta.y /= Screen.width / Screen.height;
                // Move
                Move(-delta * moveMultiplier);
            }
            else if(Input.touchCount == 2)
            {
                // If there are 2 touches, zoom.
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 t0Pos = NormalizeTouchPoint(t0.position);
                Vector2 t1Pos = NormalizeTouchPoint(t1.position);
                Vector2 t0Prev = NormalizeTouchPoint(t0.position - t0.deltaPosition);
                Vector2 t1Prev = NormalizeTouchPoint(t1.position - t1.deltaPosition);

                float prevTouchDistance = Vector2.Distance(t0Prev, t1Prev);
                float touchDistance = Vector2.Distance(t0Pos, t1Pos);
                float deltaDistance = prevTouchDistance - touchDistance;
                ZoomToViewportCenter(deltaDistance * zoomMultiplier );
            }
        }
#if UNITY_EDITOR
        else
        {
            if(Input.GetMouseButton(0))
            {
                if (!Input.GetMouseButtonDown(0))
                {
                    // Move based on lastMousePosition
                    Vector2 deltaPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - lastMousePosition;
                    Move(-deltaPosition * mouseSpeedMultiplier);
                }

                // Update the last mouse position
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                // If the mouse wheel was used, zoom.
                float mouseScroll = Input.mouseScrollDelta.y;
                if (!Mathf.Approximately(mouseScroll, 0.0f))
                {
                    ZoomToViewportCenter(-mouseScroll * mouseZoomMultiplier);
                }
            }
        }
#endif
    }

    private Vector2 NormalizeTouchPoint(Vector2 position)
    {
        return new Vector2(position.x / Screen.width, position.y / Screen.height);
    }

    private void Move(Vector2 delta)
    {
        Transform camera = LevelInstance.Instance.MainCamera.transform;
        Vector3 newPosition = ApplyCameraBounds(camera.position + new Vector3(delta.x, delta.y, 0));
        camera.position = newPosition;
    }

    private Vector3 ApplyCameraBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, cameraBounds.xMin, cameraBounds.xMax);
        position.y = Mathf.Clamp(position.y, cameraBounds.yMin, cameraBounds.yMax);
        return position;
    }

    /**
     * Zooms to the center of the screen.
     */
    private void ZoomToViewportCenter(float delta)
    {
        zoomValue = ApplyZoomBounds(zoomValue + delta);
        float perspectiveZoomValue = ApplyPerspectiveZoom(zoomValue);
        LevelInstance.Instance.MainCamera.orthographicSize = perspectiveZoomValue * originalZ;
        onZoomValueChanged?.Invoke(zoomValue);
    }

    private float ApplyZoomBounds(float value)
    {
        // Because we apply perspective zoom later (squared value), the zoom bounds represent the squared value bounds.
        // For the normal zoom value, we therefore need the square roots.
        return Mathf.Clamp(value, Mathf.Sqrt(zoomBounds.x), Mathf.Sqrt(zoomBounds.y));
    }

    private float ApplyPerspectiveZoom(float value)
    {
        // Applies perspective zoom so that each zoom step looks the same
        // (since it's 2D, zooming the same distance closer to the sprites looks like a huge step, zooming far away looks small).
        return Mathf.Clamp(value * value * Mathf.Sign(value), zoomBounds.x, zoomBounds.y);
    }
}
