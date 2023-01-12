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

        Vector2 leftUIPosition = new Vector2(dialogRect.rect.xMin - marginSide / 2, 0);
        Vector2 rightUIPosition = new Vector2(dialogRect.rect.xMax + marginSide / 2, 0);

        left.position = canvasRect.TransformPoint(leftUIPosition);
        right.position = canvasRect.TransformPoint(rightUIPosition);
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

    public void OnDialogLine(DialogLine line)
    {
        if(line.IsLeft)
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

    public void OnDialogDecision(DialogDecision decision)
    {
        if(rightAnimController)
        {
            rightAnimController.TalkIfNotTalking();
        }
    }
}
