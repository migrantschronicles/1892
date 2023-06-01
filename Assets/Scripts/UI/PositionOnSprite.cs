//#define UPDATE_POSITION_IN_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PositionOnSpriteMode
{
    Sprite,
    PlayableCharacter,
    WorldObject,
    SpriteContainer
}

/**
 * Since ui elements are positioned differently than world object like SpriteRenderer, 
 * this script allows you to position ui elements on top of world elements.
 * It provides 4 different modes, but all use the normalizedPosition.
 * If you select Sprite, then it will use the specified sprite and position the ui element (on which the component was added) 
 * on top of the sprite in the world.
 * (0/0) is the bottom left corner of the sprite, (1/1) the top right corner.
 * If you select PlayableCharacter, it will position the ui element on the player that is spawned at game start.
 * If you select WorldObject, it will only use the world location of the worldObject, assuming it has no sprite renderer / bounds.
 * You can still modify the position with worldObjectOffset and worldObjectOffsetLocal.
 * If you select SpriteContainer, it will iterate through all child SpriteRenderers of the selected game object, and 
 * calculate the bounds of the whole. Then it will use normalizedPosition as if it was one sprite.
 */
public class PositionOnSprite : MonoBehaviour
{
    [SerializeField]
    private PositionOnSpriteMode mode = PositionOnSpriteMode.Sprite;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private Vector2 normalizedPosition = new Vector2(0.5f, 0.5f);
    [SerializeField]
    private GameObject worldObject;
    [SerializeField, Tooltip("The offset applied to the screen coordinates after the world location was transformed to screen coordinates")]
    private Vector2 worldObjectOffset = new Vector2(0, 0);
    [SerializeField, Tooltip("The offset applied to the world location before it is transformed to screen coordinates")]
    private Vector3 worldObjectOffsetLocal = new Vector3(0, 0, 0);
    [SerializeField]
    private GameObject spriteContainer;

    private RectTransform rectTransform;

    public PositionOnSpriteMode Mode { get { return mode; } set { mode = value; } }
    public SpriteRenderer Sprite { get { return sprite; } set { sprite = value; } }
    public Vector2 NormalizedPosition { get { return normalizedPosition; } set { normalizedPosition = value; } }
    public GameObject WorldObject { get { return worldObject; } set { worldObject = value; } }
    public Vector2 WorldObjectOffset { set { worldObjectOffset = value; UpdatePosition(); } }
    public Vector3 WorldObjectOffsetLocal { set { worldObjectOffsetLocal = value; UpdatePosition(); } }

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
            case PositionOnSpriteMode.WorldObject:
            case PositionOnSpriteMode.SpriteContainer:
                UpdatePosition();
                break;

            case PositionOnSpriteMode.PlayableCharacter:
                LevelInstance.Instance.onPlayableCharacterSpawnChanged += OnPlayableCharacterSpawnChanged;
                if(LevelInstance.Instance.PlayableCharacterSpawn)
                {
                    OnPlayableCharacterSpawnChanged(LevelInstance.Instance.PlayableCharacterSpawn);
                }
                break;
        }
    }
    
    private bool IsInSceneInteractables(Scene scene)
    {
        Transform parent = transform.parent;
        while(parent != null)
        {
            // On the ship, Interactables is null for the scene.
            if(!scene.Interactables || parent.gameObject == scene.Interactables)
            {
                return true;
            }
        }

        return false;
    }

    private void OnPlayableCharacterSpawnChanged(PlayableCharacterSpawn spawn)
    {
        // Check if it's the scene we are in.
        // If it's a normal level (not Ship), this button could be on a scene which is not the currently opened,
        // so wait until it's the correct one.
        // On a ship, this will always be true.
        if(LevelInstance.Instance.CurrentScene && IsInSceneInteractables(LevelInstance.Instance.CurrentScene))
        {
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

    public void UpdatePosition()
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
                if (LevelInstance.Instance.CurrentScene && IsInSceneInteractables(LevelInstance.Instance.CurrentScene))
                {
                    GameObject spawnedCharacter = LevelInstance.Instance.PlayableCharacterSpawn.SpawnedCharacter;
                    UpdatePositionToSpriteContainer(spawnedCharacter);
                }

                break;
            }

            case PositionOnSpriteMode.WorldObject:
            {
                if(worldObject)
                {
                    UpdatePositionToWorldPosition(worldObject.transform.position);
                }
                break;
            }

            case PositionOnSpriteMode.SpriteContainer:
            {
                if(spriteContainer)
                {
                    UpdatePositionToSpriteContainer(spriteContainer);
                }
                break;
            }
        }
    }

    public static Bounds CalculateSpriteContainerBounds(GameObject container)
    {
        SpriteRenderer[] sprites = container.GetComponentsInChildren<SpriteRenderer>();
        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;
        for(int i = 0; i < sprites.Length; i++)
        {
            SpriteRenderer sprite = sprites[i];
            if (i == 0 || sprite.bounds.min.x < minX)
            {
                minX = sprite.bounds.min.x;
            }
            if (i == 0 || sprite.bounds.max.x > maxX)
            {
                maxX = sprite.bounds.max.x;
            }
            if (i == 0 || sprite.bounds.min.y < minY)
            {
                minY = sprite.bounds.min.y;
            }
            if (i == 0 || sprite.bounds.max.y > maxY)
            {
                maxY = sprite.bounds.max.y;
            }
        }

        Vector3 center = new(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, 0);
        Vector3 size = new(maxX - minX, maxY - minY, 0);
        return new Bounds(center, size);
    }

    private void UpdatePositionToSpriteContainer(GameObject container)
    {
        Bounds bounds = CalculateSpriteContainerBounds(container);
        UpdatePositionToBounds(bounds);
    }

    private void UpdatePositionToBounds(Bounds bounds)
    {
        Vector2 minAnchoredPosition = WorldToAnchoredPosition(bounds.min);
        Vector2 maxAnchoredPosition = WorldToAnchoredPosition(bounds.max);
        Vector2 interpolatedPosition = new Vector2(
            Mathf.LerpUnclamped(minAnchoredPosition.x, maxAnchoredPosition.x, normalizedPosition.x),
            Mathf.LerpUnclamped(minAnchoredPosition.y, maxAnchoredPosition.y, normalizedPosition.y)
            );

        rectTransform.anchoredPosition = interpolatedPosition;
    }

    private Vector2 WorldToAnchoredPosition(Vector2 worldPosition)
    {
        //https://answers.unity.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
        Vector2 viewportPosition = LevelInstance.Instance.MainCamera.WorldToViewportPoint(worldPosition);
        Vector2 screenPosition = new Vector2(
            ((viewportPosition.x * LevelInstance.Instance.CanvasRect.sizeDelta.x) - (LevelInstance.Instance.CanvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * LevelInstance.Instance.CanvasRect.sizeDelta.y) - (LevelInstance.Instance.CanvasRect.sizeDelta.y * 0.5f))
        );
        return screenPosition;
    }

    private void UpdatePositionToWorldPosition(Vector3 worldPosition)
    {
        rectTransform.anchoredPosition = WorldToAnchoredPosition(worldPosition + worldObjectOffsetLocal) + worldObjectOffset;
    }
}
