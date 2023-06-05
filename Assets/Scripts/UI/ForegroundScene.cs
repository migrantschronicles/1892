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
    private CharacterDialogInfo leftDialogInfo;
    private GameObject rightCharacter;
    private IAnimationController[] rightAnimController;
    private CharacterDialogInfo rightDialogInfo;

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
            SetScaleToWorldSize(leftCharacter, worldWidth, worldHeight, leftDialogInfo);
        }

        if (rightCharacter)
        {
            SetScaleToWorldSize(rightCharacter, worldWidth, worldHeight, rightDialogInfo);
        }
    }

    private void SetScaleToWorldSize(GameObject character, float worldWidth, float worldHeight, CharacterDialogInfo dialogInfo)
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
        scaleFactor *= dialogInfo.ScaleFactor;
        parent.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        // Reparent
        character.transform.SetParent(parent, false);
    }

    public void SetCharacters(string leftTechnicalName, string rightTechnicalName)
    {
        if(!string.IsNullOrEmpty(leftTechnicalName))
        {
            CharacterDialogInfo dialogInfo = NewGameManager.Instance.CharacterManager.GetCharacterInfo(leftTechnicalName);
            UpdateCharacter(ref leftCharacter, ref leftDialogInfo, ref leftAnimController, dialogInfo, left);
        }
        else if(leftCharacter)
        {
            Destroy(leftCharacter);
            leftCharacter = null;
            leftDialogInfo = null;
        }

        if(!string.IsNullOrEmpty(rightTechnicalName))
        {
            CharacterDialogInfo dialogInfo = NewGameManager.Instance.CharacterManager.GetCharacterInfo(rightTechnicalName);
            UpdateCharacter(ref rightCharacter, ref rightDialogInfo, ref rightAnimController, dialogInfo, right);
        }
        else if(rightCharacter)
        {
            Destroy(rightCharacter);
            rightCharacter = null;
            rightDialogInfo = null;
        }
    }

    private void UpdateCharacter(ref GameObject character, ref CharacterDialogInfo savedDialogInfo, ref IAnimationController[] animController, 
        CharacterDialogInfo dialogInfo, Transform parent)
    {
        if(savedDialogInfo != dialogInfo)
        {
            if(character)
            {
                Destroy(character);
                character = null;
                savedDialogInfo = null;
            }

            if(dialogInfo != null)
            {
                character = Instantiate(dialogInfo.Prefab, parent);
                savedDialogInfo = dialogInfo;
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
        CharacterDialogInfo dialogInfo = NewGameManager.Instance.CharacterManager.GetCharacterInfo(speakerTechnicalName);
        IEnumerable<IAnimationController> animControllers;
        if(DialogSystem.Instance.IsRight(speakerTechnicalName))
        {
            UpdateCharacter(ref rightCharacter, ref rightDialogInfo, ref rightAnimController, dialogInfo, right);
            animControllers = rightAnimController;
        }
        else
        {
            UpdateCharacter(ref leftCharacter, ref leftDialogInfo, ref leftAnimController, dialogInfo, left);
            animControllers = leftAnimController;
        }

        foreach(IAnimationController controller in animControllers)
        {
            controller.TalkIfNotTalking();
        }
    }

    public void OnDialogDecision()
    {
        ProtagonistData mainProtagonist = NewGameManager.Instance.PlayableCharacterData.GetMainProtagonist();
        CharacterDialogInfo dialogInfo = NewGameManager.Instance.CharacterManager.GetCharacterInfo(mainProtagonist.technicalName);
        UpdateCharacter(ref rightCharacter, ref rightDialogInfo, ref rightAnimController, dialogInfo, right);
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
