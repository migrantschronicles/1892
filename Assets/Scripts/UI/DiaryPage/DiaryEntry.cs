using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class DiaryPageData
{
    public GameObject prefab;
    public LocalizedString text;
    public Sprite image;
}

[CreateAssetMenu(fileName = "NewDiaryEntry", menuName = "ScriptableObjects/DiaryEntry", order = 1)]
public class DiaryEntry : ScriptableObject
{
    [Tooltip("True if the entry should start on a new double page, even if this means that there is a blank page on the previous double page.")]
    public bool startOnNewDoublePage;
    [Tooltip("The pages for this entry")]
    public DiaryPageData[] pages;
}
