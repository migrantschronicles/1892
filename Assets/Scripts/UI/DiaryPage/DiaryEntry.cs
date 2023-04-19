using System;
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

    public bool IsValid
    {
        get
        {
            return prefab != null;
        }
    }
}

[CreateAssetMenu(fileName = "NewDiaryEntry", menuName = "ScriptableObjects/DiaryEntry", order = 1)]
public class DiaryEntry : ScriptableObject
{
    [Tooltip("The left page for this entry")]
    public DiaryPageData leftPage;
    [Tooltip("The right page for this entry")]
    public DiaryPageData rightPage;
}

[System.Serializable]
public class DiaryEntryData
{
    public DiaryEntry entry;
    public string textLeft;
    public string textLeft2;
    public string textRight;
    public string textRight2;
    public string localizedText;
    public DateTime date;

    public string LocalizedDate
    {
        get
        {
            return date != null ? date.ToString("d MMMM yyyy") : "";
        }
    }

    public bool HasOverrides
    {
        get
        {
            return !string.IsNullOrEmpty(textLeft) || !string.IsNullOrEmpty(textLeft2) || !string.IsNullOrEmpty(textRight) ||
                !string.IsNullOrEmpty(textRight); 
        }
    }
    
    public string GetTextForPage(DiaryPageData page)
    {
        if(page == entry.leftPage)
        {
            return textLeft;
        }
        else if(page == entry.rightPage)
        {
            return textRight;
        }

        return "";
    }

    public string GetText2ForPage(DiaryPageData page)
    {
        if (page == entry.leftPage)
        {
            return textLeft2;
        }
        else if (page == entry.rightPage)
        {
            return textRight2;
        }

        return "";
    }
}
