//#define UPDATE_POSITION_IN_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PositionOnSpriteMode
{
    Sprite,
    PlayableCharacter
}

public class PositionOnSprite : MonoBehaviour
{
    [SerializeField]
    private PositionOnSpriteMode mode = PositionOnSpriteMode.Sprite;
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
        switch(mode)
        {
            case PositionOnSpriteMode.Sprite:
                UpdatePosition();
                break;

            case PositionOnSpriteMode.PlayableCharacter:
                LevelInstance.Instance.onSceneChanged += OnSceneChanged;
                if(LevelInstance.Instance.CurrentScene)
                {
                    OnSceneChanged(LevelInstance.Instance.CurrentScene);
                }
                break;
        }
    }
    
    private bool IsInSceneInteractables(Scene scene)
    {
        Transform parent = transform.parent;
        while(parent != null)
        {
            if(parent.gameObject == scene.Interactables)
            {
                return true;
            }
        }

        return false;
    }

    private void OnSceneChanged(Scene scene)
    {
        if(scene && IsInSceneInteractables(scene))
        {
            LevelInstance.Instance.onSceneChanged -= OnSceneChanged;
            UpdatePosition();
        }
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
        switch(mode)
        {
            case PositionOnSpriteMode.Sprite:
            {
                if (sprite)
                {
                    UpdatePositionToBounds(sprite.bounds);
                }
                break;
            }

            case PositionOnSpriteMode.PlayableCharacter:
            {
                GameObject spawnedCharacter = LevelInstance.Instance.CurrentScene.SpawnedCharacter;
                SpriteRenderer[] renderers = spawnedCharacter.GetComponentsInChildren<SpriteRenderer>(true);
                Bounds? bounds = null;
                foreach(SpriteRenderer renderer in renderers)
                {
                    if(bounds != null)
                    {
                        bounds.Value.Encapsulate(renderer.bounds);
                    }
                    else
                    {
                        bounds = renderer.bounds;
                    }
                }

                if(bounds != null)
                {
                    UpdatePositionToBounds(bounds.Value);
                }

                break;
            }
        }
    }

    private void UpdatePositionToBounds(Bounds bounds)
    {
        Vector2 minAnchoredPosition = WorldToAnchoredPosition(bounds.min);
        Vector2 maxAnchoredPosition = WorldToAnchoredPosition(bounds.max);
        Vector2 interpolatedPosition = new Vector2(
            Mathf.Lerp(minAnchoredPosition.x, maxAnchoredPosition.x, normalizedPosition.x),
            Mathf.Lerp(minAnchoredPosition.y, maxAnchoredPosition.y, normalizedPosition.y)
            );

        rectTransform.anchoredPosition = interpolatedPosition;
    }

    private Vector2 WorldToAnchoredPosition(Vector2 worldPosition)
    {
        Vector2 anchoredPosition = LevelInstance.Instance.CanvasRect.InverseTransformPoint(worldPosition);
        return anchoredPosition;
    }
}
