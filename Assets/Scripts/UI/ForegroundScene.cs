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
    [SerializeField]
    private Vector2 worldPadding = new Vector2(0.5f, 0.0f);

    private GameObject leftCharacter;
    private IAnimationController[] leftAnimController;
    private GameObject rightCharacter;
    private IAnimationController[] rightAnimController;

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
        Vector2 localMin = new(-UI_SIZE.x / 2, dialogRect.rect.yMin);
        Vector2 localMax = new(dialogRect.rect.xMin, dialogRect.rect.yMax);
        Vector3 worldMin = canvasRect.TransformPoint(localMin);
        Vector3 worldMax = canvasRect.TransformPoint(localMax);
        float worldWidth = worldMax.x - worldMin.x - worldPadding.x;
        float worldHeight = worldMax.y - worldMin.y - worldPadding.y;

        if(leftCharacter)
        {
            SetScaleToWorldSize(leftCharacter, worldWidth, worldHeight);
        }

        if (rightCharacter)
        {
            SetScaleToWorldSize(rightCharacter, worldWidth, worldHeight);
        }
    }

    private void SetScaleToWorldSize(GameObject character, float worldWidth, float worldHeight)
    {
        // Detach parent so that scale does not modify result.
        Transform parent = character.transform.parent;
        character.transform.SetParent(null, false);

        // Calculate the sprite height (since the characters are made of a lot of sprites, all have to be considered).
        Bounds bounds = PositionOnSprite.CalculateSpriteContainerBounds(character);
        Vector2 spriteSize = bounds.size;
        float widthScaleFactor = worldWidth / spriteSize.x;
        float heightScaleFactor = worldHeight / spriteSize.y;
        float scaleFactor = Mathf.Min(widthScaleFactor, heightScaleFactor);
        parent.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // Reparent
        character.transform.SetParent(parent, false);
    }

    public void SetCharacters(GameObject leftPrefab, GameObject rightPrefab)
    {
        UpdateCharacter(ref leftCharacter, ref leftAnimController, leftPrefab, left);
        UpdateCharacter(ref rightCharacter, ref rightAnimController, rightPrefab, right);
    }

    private void UpdateCharacter(ref GameObject character, ref IAnimationController[] animController, GameObject prefab, Transform parent)
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
                animController = character.GetComponentsInChildren<IAnimationController>();
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

    public void OnDialogLine(string speakerTechnicalName)
    {
        if(!DialogSystem.Instance.IsRight(speakerTechnicalName))
        {
            foreach(IAnimationController controller in leftAnimController)
            {
                controller.TalkIfNotTalking();
            }
        }
        else
        {
            foreach (IAnimationController controller in rightAnimController)
            {
                controller.TalkIfNotTalking();
            }
        }
    }

    public void OnDialogDecision()
    {
        foreach (IAnimationController controller in rightAnimController)
        {
            controller.TalkIfNotTalking();
        }
    }

    public bool HasRightCharacter(string technicalName)
    {
        if(rightCharacter)
        {
            ProtagonistData data = NewGameManager.Instance.PlayableCharacterData.GetProtagonistDataByTechnicalName(technicalName);
            if(data != null)
            {
                ProtagonistAnimationController[] controllers = rightCharacter.GetComponentsInChildren<ProtagonistAnimationController>();
                foreach (ProtagonistAnimationController controller in controllers)
                {
                    string protagonistName = controller.ProtagonistName;
                    if(protagonistName == data.name)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
