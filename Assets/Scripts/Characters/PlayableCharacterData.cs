using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneCharacterPrefab
{
    [Tooltip("The prefab that will be instantiated into the scene.")]
    public GameObject prefab;
    [Tooltip("The name of the specific prefab (can be used to have different arrangements of characters).")]
    public string name;
}

[System.Serializable]
public class ProtagonistData
{
    [Tooltip("The name that will be displayed")]
    public string name;
    [Tooltip("Whether this is the main protagonist")]
    public bool isMainProtagonist;
    [Tooltip("The technical name in Articy")]
    public string technicalName;
    public Sprite neutralPortrait;
    public Sprite angryPortrait;
    public Sprite happyPortrait;
    public Sprite hungryPortrait;
    public Sprite sadPortrait;
    public Sprite sickPortrait;
}

/**
 * This is the data for a playable character (e.g. the family, in this case multiple).
 * You can set the conditions that will be set globally if this character is selected in the main menu.
 * You can specify the prefab that will be displayed to the right of the dialog.
 * E.g. for the family you will have the mother and the children in the scene, but in the dialog you only want to have the mother.
 * You can have multiple prefabs that will be displayed in the scene.
 * E.g. if you want to have one scene where the family is facing left in the order child 1 / mother / child 2,
 * and another scene where the family stands facing right in the order mother / child 1 / child2,
 * you can create different prefabs for them and add multiple ScenePrefabs, each with a unique name.
 * Then in the PlayableCharacterSpawn component, you can set the name of the prefab you want to spawn,
 * so for each scene you can have a different layout / arrangement of characters.
 */
[CreateAssetMenu(fileName = "NewPlayableCharacter", menuName = "ScriptableObjects/Playable Character", order = 1)]
public class PlayableCharacterData : ScriptableObject
{
    [Tooltip("The conditions that will be set when the character is selected.")]
    public SetCondition[] setConditions;
    [Tooltip("The prefab that will be displayed on the right side of the dialogs.")]
    public GameObject dialogPrefab;
    [Tooltip("The prefabs that will be instantiated into the scene.")]
    public SceneCharacterPrefab[] scenePrefabs;
    [Tooltip("The data about the characters")]
    public ProtagonistData[] protagonistData;
}
