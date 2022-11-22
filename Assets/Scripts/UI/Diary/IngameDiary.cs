using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngameDiary : MonoBehaviour
{
    [SerializeField]
    private Animator diaryAnimator;
    [SerializeField]
    private Diary diary;
    [SerializeField]
    private DiaryContentPage defaultPage;
    [SerializeField]
    private GameObject locationMarkerParent;

    private Dictionary<string, LocationMarker> locationMarkers;

    public Diary Diary { get { return diary; } }

    public Dictionary<string, LocationMarker> LocationMarkers
    {
        get
        {
            if (locationMarkers == null)
            {
                GatherLocationMarkers();
            }

            return locationMarkers;
        }
    }

    public IEnumerable<string> LocationStrings { get { return LocationMarkers.Keys; } }
    public IEnumerable<LocationMarker> LocationMarkerObjects { get { return LocationMarkers.Values; } }
    public IEnumerable<GameObject> LocationMarkersGO
    {
        get
        {
            return LocationMarkers.Values.Select(marker => marker.gameObject);
        }
    }

    private void Awake()
    {
        diary.onDiaryStatusChanged += OnDiaryStatusChanged;
        if (locationMarkers == null)
        {
            GatherLocationMarkers();
        }
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch(status)
        {
            case OpenStatus.Opening:
                diaryAnimator.SetBool("Opened", true);
                StartCoroutine(WaitForAnimationEvents());
                break;

            case OpenStatus.Closing:
                diaryAnimator.SetBool("Opened", false);
                StartCoroutine(WaitForAnimationEvents());
                break;
        }
    }

    private IEnumerator WaitForAnimationEvents()
    {
        while((diary.Status == OpenStatus.Opening && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryOpened"))
            || (diary.Status == OpenStatus.Closing && !diaryAnimator.GetCurrentAnimatorStateInfo(0).IsName("DiaryClosed")))
        {
            yield return null;
        }

        switch(diary.Status)
        {
            case OpenStatus.Opening:
                diary.OnOpeningAnimationFinished();
                break;

            case OpenStatus.Closing:
                diary.OnClosingAnimationFinished();
                break;
        }
    }

    public void SetOpened(bool opened)
    {
        if(opened)
        {
            diary.SetOpened(defaultPage);
        }
        else
        {
            diary.SetOpened(opened);
        }
    }

    private void GatherLocationMarkers()
    {
        locationMarkers = new Dictionary<string, LocationMarker>();
        for (int i = 0; i < locationMarkerParent.transform.childCount; ++i)
        {
            LocationMarker marker = locationMarkerParent.transform.GetChild(i).GetComponent<LocationMarker>();
            if (marker != null)
            {
                locationMarkers.Add(marker.LocationName, marker);
            }
        }
    }
}
