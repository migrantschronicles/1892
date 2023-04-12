using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Blur : MonoBehaviour
{
    private Volume volume;

    private DepthOfField dof;
    private ColorAdjustments colorAdjustments;
    private Color targetColor;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out dof);
        volume.profile.TryGet(out colorAdjustments);
        targetColor = colorAdjustments.colorFilter.value;
    }

    /**
     * Sets the blur (depth of field and darken) enabled.
     * Resets the fade amount.
     */
    public void SetEnabled(bool enabled)
    {
        gameObject.SetActive(enabled);

        if(enabled)
        {
            dof.active = true;
            colorAdjustments.colorFilter.value = targetColor;
        }
    }
}
