using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopFlag : MonoBehaviour
{
    [SerializeField]
    private string sceneName;
    [SerializeField]
    private SpriteRenderer poleRenderer;
    [SerializeField]
    private SpriteRenderer flagRenderer;
    [SerializeField]
    private bool isLeft = true;
    [SerializeField]
    private Vector3 leftFlagPosition;
    [SerializeField]
    private Vector3 rightFlagPosition;
    [SerializeField]
    private Sprite leftPole;
    [SerializeField]
    private Sprite rightPole;
    [SerializeField]
    private Sprite defaultFlag;
    [SerializeField]
    private Sprite pressedFlag;

    private void Start()
    {
        UpdateElements();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateElements();
    }
#endif

    private void UpdateElements()
    {
        poleRenderer.sprite = isLeft ? leftPole : rightPole;
        flagRenderer.transform.localPosition = isLeft ? leftFlagPosition : rightFlagPosition;
    }

    private void OnMouseDown()
    {
        flagRenderer.sprite = pressedFlag;
    }

    private void OnMouseEnter()
    {
        flagRenderer.sprite = pressedFlag;
    }

    private void OnMouseExit()
    {
        flagRenderer.sprite = defaultFlag;
    }

    private void OnMouseUp()
    {
        flagRenderer.sprite = defaultFlag;
    }

    private void OnMouseUpAsButton()
    {
        flagRenderer.sprite = defaultFlag;
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            LevelInstance.Instance.OpenScene(sceneName);
        }
    }
}
