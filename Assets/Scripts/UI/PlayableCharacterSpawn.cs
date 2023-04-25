using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayableCharacterSpawn : MonoBehaviour
{
    [SerializeField, Tooltip("The name of the prefab to spawn, or leave empty for default.")]
    private string prefabName;

    public GameObject SpawnedCharacter { get; private set; }

    public bool TrySpawn()
    {
        if(!SpawnedCharacter)
        {
            PlayableCharacterData data = NewGameManager.Instance.PlayableCharacterData;
            SceneCharacterPrefab scenePrefab = data.scenePrefabs.First((prefab) => prefab.name == prefabName);
            if (scenePrefab != null)
            {
                SpawnedCharacter = Instantiate(scenePrefab.prefab, transform);
                return true;
            }
        }

        return false;
    }

    public void DestroyCharacters()
    {
        if(SpawnedCharacter)
        {
            Destroy(SpawnedCharacter);
            SpawnedCharacter = null;
        }
    }

    public void SetCharactersVisible(bool visible)
    {
        if(SpawnedCharacter)
        {
            SpawnedCharacter.SetActive(visible);
        }
    }
}
