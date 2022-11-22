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
        set
        {
            if(diaryMarker != null)
            {
                diaryMarker.SetActive(value);
            }

            foreach(GameObject requiredObject in requiredObjects)
            {
                requiredObject.SetActive(value);
            }

            onActiveStatusChanged?.Invoke(value);
        }
    }
}
