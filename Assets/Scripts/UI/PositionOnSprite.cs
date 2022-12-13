//#define UPDATE_POSITION_IN_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOnSprite : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private Vector2 normalizedPosition = new Vector2(0.5f, 0.5f);

    private RectTransform rectTransform;

#if UNITY_EDITOR && UPDATE_POSITION_IN_EDITOR
    private Vector2Int lastResolution;
#endif

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
#if UNITY_EDITOR && UPDATE_POSITION_IN_EDITOR
        lastResolution = new Vector2Int(Screen.width, Screen.height);
#endif
    }

    private void Start()
    {
        UpdatePosition();
    }

#if UNITY_EDITOR && UPDATE_POSITION_IN_EDITOR
    private void Update()
    {
        if(lastResolution.x != Screen.width || lastResolution.y != Screen.height)
        {
            UpdatePosition();
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
        }
    }
#endif

    private void UpdatePosition()
    {
        if(sprite)
        {
            Vector2 minAnchoredPosition = WorldToAnchoredPosition(sprite.bounds.min);
            Vector2 maxAnchoredPosition = WorldToAnchoredPosition(sprite.bounds.max);
            Vector2 interpolatedPosition = new Vector2(
                Mathf.Lerp(minAnchoredPosition.x, maxAnchoredPosition.x, normalizedPosition.x),
                Mathf.Lerp(minAnchoredPosition.y, maxAnchoredPosition.y, normalizedPosition.y)
                );

            rectTransform.anchoredPosition = interpolatedPosition;
        }
    }

    private Vector2 WorldToAnchoredPosition(Vector2 worldPosition)
    {
        Vector2 anchoredPosition = LevelInstance.Instance.CanvasRect.InverseTransformPoint(worldPosition);
        return anchoredPosition;
    }
}
