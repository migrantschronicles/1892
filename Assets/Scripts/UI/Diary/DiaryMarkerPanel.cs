using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryMarkerPanel : MonoBehaviour
{
    private DiaryMarker[] markers;

    private void Awake()
    {
        markers = GetComponentsInChildren<DiaryMarker>();
    }

    public void SetClosed(bool closed)
    {
        foreach(DiaryMarker marker in markers)
        {
            marker.SetClosed(closed);
        }    
    }
}
