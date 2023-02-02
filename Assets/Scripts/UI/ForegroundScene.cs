using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForegroundScene : MonoBehaviour
{
    private static readonly Vector2 UI_SIZE = new Vector2(1920, 1200);

    [SerializeField]
    private Transform left;
    [SerializeField]
    private Transform right;

    private GameObject leftCharacter;
    private IAnimationController leftAnimController;
    private GameObject rightCharacter;
    private IAnimationController rightAnimController;

    private void Start()
    {
        UpdatePosition();
    }

    private void Update()
    {
        transform.position = new Vector3(LevelInstance.Instance.MainCamera.transform.position.x, 
            LevelInstance.Instance.MainCamera.transform.position.y, 0.0f);
    }

    private void UpdatePosition()
    {
        RectTransform dialogRect = DialogSystem.Instance.GetComponent<RectTransform>();
        RectTransform canvasRect = LevelInstance.Instance.CanvasRect;
        float marginSide = UI_SIZE.x / 2 + dialogRect.rect.xMin;

        Vector2 leftUIPosition = new(dialogRect.rect.xMin - marginSide / 2, 0);
        Vector2 rightUIPosition = new(dialogRect.rect.xMax + marginSide / 2, 0);

        left.position = canvasRect.TransformPoint(leftUIPosition);
        right.position = canvasRect.TransformPoint(rightUIPosition);

        // Update scale
        Vector2 localMin = new(0, dialogRect.rect.yMin);
        Vector2 localMax = new(0, dialogRect.rect.yMax);
        Vector3 worldMin = canvasRect.TransformPoint(localMin);
        Vector3 worldMax = canvasRect.TransformPoint(localMax);
        float worldHeight = worldMax.y - worldMin.y;

        if(leftCharacter)
        {
            SetScaleToWorldHeight(leftCharacter, worldHeight);
        }

        if (rightCharacter)
        {
            SetScaleToWorldHeight(rightCharacter, worldHeight);
        }
    }

    private void SetScaleToWorldHeight(GameObject character, float worldHeight)
    {
        // Detach parent so that scale does not modify result.
        Transform parent = character.transform.parent;
        character.transform.SetParent(null, false);

        // Calculate the sprite height (since the characters are made of a lot of sprites, all have to be considered).
        float spriteHeight = CalculateSpriteHeight(character);
        float scaleFactor = worldHeight / spriteHeight;
        parent.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // Reparent
        character.transform.SetParent(parent, false);
    }

    private float CalculateSpriteHeight(GameObject go)
    {
        float minY = 0;
        float maxY = 0;
        SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sprites.Length; ++i)
        {
            SpriteRenderer sprite = sprites[i];
            if(i == 0 || sprite.bounds.min.y < minY)
            {
                minY = sprite.bounds.min.y;
            }
            if(i == 0 || sprite.bounds.max.y > maxY)
            {
                maxY = sprite.bounds.max.y;
            }
        }

        return maxY - minY;
    }

    public void SetCharacters(GameObject leftPrefab, GameObject rightPrefab)
    {
        UpdateCharacter(ref leftCharacter, ref leftAnimController, leftPrefab, left);
        UpdateCharacter(ref rightCharacter, ref rightAnimController, rightPrefab, right);
    }

    private void UpdateCharacter(ref GameObject character, ref IAnimationController animController, GameObject prefab, Transform parent)
    {
        if(character != prefab)
        {
            if(character)
            {
                Destroy(character);
                character = null;
            }

            if(prefab)
            {
                character = Instantiate(prefab, parent);
                SetLayer(character, LayerMask.NameToLayer("Foreground"));
                animController = character.GetComponent<IAnimationController>();
                UpdatePosition();
            }
        }
    }

    private void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        for(int i = 0; i < go.transform.childCount; ++i)
        {
            SetLayer(go.transform.GetChild(i).gameObject, layer);
        }
    }

    public void OnDialogLine(bool isMainProtagonist)
    {
        if(!isMainProtagonist)
        {
            if(leftAnimController)
            {
                leftAnimController.TalkIfNotTalking();
            }
        }
        else
        {
            if(rightAnimController)
            {
                rightAnimController.TalkIfNotTalking();
            }
        }
    }

    public void OnDialogDecision()
    {
        if(rightAnimController)
        {
            rightAnimController.TalkIfNotTalking();
        }
    }
}
