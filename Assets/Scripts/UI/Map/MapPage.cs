using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPage : MonoBehaviour
{
    [SerializeField]
    private GameObject controlPanel;
    [SerializeField]
    private Mask mask;

    public void PrepareForMapScreenshot()
    {
        controlPanel.SetActive(false);
        mask.enabled = false;
    }

    public void ResetFromScreenshot()
    {
        controlPanel.SetActive(true);
        mask.enabled = true;
    }
}
