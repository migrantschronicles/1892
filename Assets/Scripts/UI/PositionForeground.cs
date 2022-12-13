using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionForeground : MonoBehaviour
{
    private static readonly Vector2 UI_SIZE = new Vector2(1920, 1200);

    [SerializeField]
    private Transform left;
    [SerializeField]
    private Transform right;

    private void Start()
    {
        UpdatePosition();
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
}
