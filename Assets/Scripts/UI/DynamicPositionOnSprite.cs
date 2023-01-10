using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Updates the position and scale every frame. Only needed for the ship, since the camera is moving there.
 */
[RequireComponent(typeof(PositionOnSprite))]
public class DynamicPositionOnSprite : MonoBehaviour
{
    private PositionOnSprite positionOnSprite;
    private ShipMovement shipMovement;
    private RectTransform rectTransform;

    private void Awake()
    {
        positionOnSprite = GetComponent<PositionOnSprite>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        shipMovement = LevelInstance.Instance.GetComponent<ShipMovement>();
        shipMovement.onZoomValueChanged += OnZoomValueChanged;
        OnZoomValueChanged(shipMovement.ZoomValue);
    }

    private void Update()
    {
        positionOnSprite.UpdatePosition();
    }

    private void OnZoomValueChanged(float zoomValue)
    {
        float scale = 1.0f / zoomValue;
        rectTransform.localScale = new Vector3(scale, scale, scale);
    }
}
