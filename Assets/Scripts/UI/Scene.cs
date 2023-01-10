using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    [SerializeField]
    private GameObject interactables;
    [SerializeField]
    private string sceneName;
    [SerializeField]
    private PlayableCharacterSpawn characterSpawn;

    public string SceneName { get { return sceneName; } }
    public GameObject SpawnedCharacter { get { return characterSpawn.SpawnedCharacter; } }
    public GameObject Interactables { get { return interactables; } }

    private void Start()
    {
        if(LevelInstance.Instance.LevelMode == LevelInstanceMode.Default)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("Scene name is empty in: " + name);
            }

            if (characterSpawn == null)
            {
                Debug.LogError("No character spawn is set in: " + name);
            }
        }
    }

    public void OnActiveStatusChanged(bool active)
    {
        if(interactables)
        {
            interactables.SetActive(active);
        }

        if(active && characterSpawn)
        {
            characterSpawn.TrySpawn();
        }
    }

    public void SetPlayableCharacterVisible(bool visible)
    {
        characterSpawn.SetCharactersVisible(visible);
    }
}
