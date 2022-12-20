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

[CreateAssetMenu(fileName = "NewPlayableCharacter", menuName = "ScriptableObjects/Playable Character", order = 1)]
public class PlayableCharacterData : ScriptableObject
{
    [Tooltip("The conditions that will be set when the character is selected.")]
    public SetCondition[] setConditions;
    [Tooltip("The prefab that will be displayed on the right side of the dialogs.")]
    public GameObject dialogPrefab;
    [Tooltip("The prefabs that will be instantiated into the scene.")]
    public SceneCharacterPrefab[] scenePrefabs;
}
