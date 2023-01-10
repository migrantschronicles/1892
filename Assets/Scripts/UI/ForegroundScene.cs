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
    private GameObject rightCharacter;

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
        UpdateCharacter(ref leftCharacter, leftPrefab, left);
        UpdateCharacter(ref rightCharacter, rightPrefab, right);
    }

    private void UpdateCharacter(ref GameObject character, GameObject prefab, Transform parent)
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
}
