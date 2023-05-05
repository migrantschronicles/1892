using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapFragment : MonoBehaviour
{
    private MapZoom mapZoom;
    private RectTransform rectTransform;
    private Image image;
    private int level = -1;
    private Rect? contentRect;

    public int Level
    {
        get
        {
            if(level < 0)
            {
                MapFragment parent = transform.parent.GetComponent<MapFragment>();
                if(parent != null)
                {
                    level = parent.Level + 1;
                }
                else
                {
                    level = 0;
                }
            }

            return level;
        }
    }

    public Rect ContentRect
    {
        get
        {
            if(contentRect == null)
            {
                MapFragment parent = transform.parent.GetComponent<MapFragment>();
                if(parent != null)
                {
                    Vector2 size = rectTransform.sizeDelta;
                    Vector2 position = parent.ContentRect.position + rectTransform.anchoredPosition - size / 2;
                    contentRect = new Rect(position, size);
                }
                else
                {
                    Vector2 size = rectTransform.sizeDelta;
                    Vector2 position = rectTransform.anchoredPosition - size / 2;
                    contentRect = new Rect(position, size);
                }
            }

            return contentRect.Value;
        }
    }

    public bool IsLevelVisible { get; private set; }
    public bool IsInViewport { get; private set; }
    public bool IsVisible 
    { 
        get 
        {
            return IsLevelVisible && IsInViewport;
        } 
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        mapZoom = GetComponentInParent<MapZoom>();
        mapZoom.onMapZoomChangedEvent += OnMapZoomChanged;
        OnMapZoomChanged(mapZoom.ZoomLevel);
    }

    private void Update()
    {
        // Checks if the fragment is inside of the viewport.
        IsInViewport = mapZoom.IsVisible(ContentRect);
        UpdateVisibility();
    }

    private void OnMapZoomChanged(float zoomLevel)
    {
        // Checks if the same zoom level is visible.
        IsLevelVisible = mapZoom.VisibleLevel == Level;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        // Only enable the image if both the zoom level fits and the fragment is inside the viewport.
        image.enabled = IsVisible;
    }
}
