using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryContentPages : MonoBehaviour
{
    [SerializeField]
    private DiaryMarker diaryMarker;
    [SerializeField]
    private GameObject[] requiredObjects;

    public delegate void OnActiveStatusChangedEvent(bool active);
    public event OnActiveStatusChangedEvent onActiveStatusChanged;

    private bool isActive = false;

    public DiaryMarker DiaryMarker { get { return diaryMarker; } }

    public DiaryContentPage CurrentPage { get; set; }

    public DiaryContentPage LastPage
    {
        get
        {
            return transform.childCount > 0 ? transform.GetChild(transform.childCount - 1).GetComponent<DiaryContentPage>() : null;
        }
    }

    public bool Active
    {
        get { return isActive; }
        set
        {
            ActiveSilent = value;
            onActiveStatusChanged?.Invoke(value);
        }
    }

    public bool ActiveSilent
    {
        set
        {
            isActive = value;
            if (diaryMarker != null)
            {
                diaryMarker.SetActive(value);
            }

            foreach (GameObject requiredObject in requiredObjects)
            {
                requiredObject.SetActive(value);
            }
        }
    }
}
