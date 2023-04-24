using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHomesicknessValue : MonoBehaviour
{
    [SerializeField]
    private RectTransform handle;
    [SerializeField]
    private int minXValue;
    [SerializeField]
    private int maxXValue;
    [SerializeField]
    private float value;

    public float Value { set { this.value = value; UpdateValue(); } }

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

        UpdateValue();
    }
#endif

    private void UpdateValue()
    {
        // Range 1..10
        float from1 = 1;
        float to1 = 10;
        float from2 = 1;
        float to2 = 0;
        float alpha = Mathf.Clamp01((value - from1) / (to1 - from1) * (to2 - from2) + from2);

        handle.anchoredPosition = new Vector2(Mathf.Lerp(minXValue, maxXValue, alpha), handle.anchoredPosition.y);
    }
}
