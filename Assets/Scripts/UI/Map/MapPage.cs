using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPage : MonoBehaviour
{
    [SerializeField]
    private GameObject controlPanel;

    public void PrepareForMapScreenshot()
    {
        controlPanel.SetActive(false);
    }

    public void ResetFromScreenshot()
    {
        controlPanel.SetActive(true);
    }
}
