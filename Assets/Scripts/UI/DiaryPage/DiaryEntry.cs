using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

/**
 * A predefined location where a sketch drawing can be.
 */
public enum DiaryPageDrawingLocation
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

/**
 * Data for a sketch drawing.
 */
[System.Serializable]
public class DiaryPageDrawing
{
    [Tooltip("The sprite that should be used for the sketch drawing. If it is not set, there is no drawing.")]
    public Sprite image;
    [Tooltip("True if the aspect of the image should be respected, false if it streches over the full size.")]
    public bool preserveAspect = true;
    [Tooltip("If false, the size of the provided image is used. If true, you can specify a custom size for the sketch drawing with the property size.")]
    public bool overrideSize = false;
    [Tooltip("The size that the sketch drawing should have. Only used if overrideSize is true.")]
    public Vector2 size;
    [Tooltip("The location where the drawing should be added. The position is adjusted so that the full image is visible " +
        "(i.e. BottomLeft -> The bottom left corner of the image lines up with the bottom left corner of the page)")]
    public DiaryPageDrawingLocation location;
    [Tooltip("You can offset the position in the specified location (+x is to the right, +y upwards). You can use it to add a little variation.")]
    public Vector2 offset = Vector2.zero;

    public bool IsEnabled
    {
        get
        {
            return image != null;
        }
    }
}

[System.Serializable]
public class DiaryPageData
{
    public GameObject prefab;
    public LocalizedString text;
    public LocalizedString text2;
    public Sprite image;
    [Tooltip("A list of sketch drawings that can be added to the page. If drawing.image is null, no sketch drawing is added.")]
    public DiaryPageDrawing[] drawings;

    // Not set in the inspector, but in DiaryPages. Can be used from DiaryPage to set the date.
    public string Date { get; set; }
}

[CreateAssetMenu(fileName = "NewDiaryEntry", menuName = "ScriptableObjects/DiaryEntry", order = 1)]
public class DiaryEntry : ScriptableObject
{
    [Tooltip("True if the entry should start on a new double page, even if this means that there is a blank page on the previous double page.")]
    public bool startOnNewDoublePage;
    [Tooltip("The pages for this entry")]
    public DiaryPageData[] pages;
}