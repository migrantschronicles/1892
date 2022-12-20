using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayableCharacterSpawn : MonoBehaviour
{
    [SerializeField, Tooltip("The name of the prefab to spawn, or leave empty for default.")]
    private string prefabName;

    private GameObject spawnedCharacter;

    public bool TrySpawn()
    {
        if(!spawnedCharacter)
        {
            PlayableCharacterData data = NewGameManager.Instance.PlayableCharacterData;
            SceneCharacterPrefab scenePrefab = data.scenePrefabs.First((prefab) => prefab.name == prefabName);
            if (scenePrefab != null)
            {
                spawnedCharacter = Instantiate(scenePrefab.prefab, transform);
                return true;
            }
        }

        return false;
    }
}
