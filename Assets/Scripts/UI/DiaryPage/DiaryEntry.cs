using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum DiaryEntryFormat
{
    // Only text on all pages.
    TextOnly,
    // An image on the top of the second page, text below and on the first page.
    ImageSecondTop,
    // Text on the first page, image on the full second page.
    ImageSecond
}

[CreateAssetMenu(fileName = "NewDiaryEntry", menuName = "ScriptableObjects/DiaryEntry", order = 1)]
public class DiaryEntry : ScriptableObject
{
    public DiaryEntryFormat format;
    public LocalizedString text;
    public string dateOverride;
    [Tooltip("True if the entry should start on a new double page, even if this means that there is a blank page on the previous double page.")]
    public bool startOnNewDoublePage;
}
