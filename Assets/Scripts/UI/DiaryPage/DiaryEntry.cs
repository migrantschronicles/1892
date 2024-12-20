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
    public Sprite image;
    [Tooltip("A list of sketch drawings that can be added to the page. If drawing.image is null, no sketch drawing is added.")]
    public DiaryPageDrawing[] drawings;

    private string text;
    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            OnTextChanged?.Invoke(text);
        }
    }

    private string text2;
    public string Text2
    {
        get { return text2; }
        set
        {
            text2 = value;
            OnText2Changed?.Invoke(text2);
        }
    }

    public delegate void OnTextChangedEvent(string text);
    public event OnTextChangedEvent OnTextChanged;
    public event OnTextChangedEvent OnText2Changed;

    public bool IsValid
    {
        get
        {
            return prefab != null;
        }
    }

    public DiaryPageData Clone()
    {
        DiaryPageData newData = new DiaryPageData
        {
            prefab = prefab,
            image = image,
            drawings = drawings,
            Text = Text,
            Text2 = Text2
        };
        return newData;
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

public enum HealthProblemType
{
    Cholera,
    Homesickness
}

public class HealthProblem
{
    public ProtagonistData character;
    public HealthProblemType sickness;
}

public enum GeneratedDiaryEntryPurpose
{
    NewCity,
    NewDay,
    EndGame
}

public class DiaryHealthState
{
    public ProtagonistData character;
    public HealthState healthState;
}

[System.Serializable]
public class DiaryEntryInfo
{
    public LevelInstanceMode levelMode;
    public int daysInCity;
    public string locationName;
    public bool isStopoverDay;
    public SleepMethod lastSleepMethod;
    public List<StolenItemInfo> lastStolenItems = new();
    public List<ProtagonistData> hungryCharacters = new();
    public string stopoverLocation;
    public List<DiaryHealthState> healthStates = new();
    public List<HealthProblem> newHealthProblems = new();
    public List<HealthProblem> existingHealthProblems = new();
    public long date;
    public TransportationMethod lastTransportationMethod;
    public GeneratedDiaryEntryPurpose purpose;
    public string endGameEntryTechnicalName;
    public float playtime;

    public DateTime Date
    {
        get
        {
            return DateTime.FromFileTimeUtc(date);
        }

        set
        {
            date = value.ToFileTimeUtc();
        }
    }
}

[System.Serializable]
public class DiaryEntryData
{
    public DiaryEntry entry;
    public DiaryPageData leftPage;
    public DiaryPageData rightPage;
    public DateTime date;
    public DiaryEntryInfo info;

    public delegate void OnDateChangedEvent(string date);
    public event OnDateChangedEvent OnDateChanged;

    public string LocalizedDate
    {
        get
        {
            return date != null ? date.ToString("d MMMM yyyy") : "";
        }
    }
}
