using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public PlayableCharacterSpawn PlayableCharacterSpawn { get { return characterSpawn; } }
    public int DaysInScene { get; private set; }

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

        if(characterSpawn)
        {
            if(active)
            {
                characterSpawn.TrySpawn();
            }
            else
            {
                characterSpawn.DestroyCharacters();
            }
        }

        if(active)
        {
            DaysInScene = 0;
            NewGameManager.Instance.onNewDay += OnNewDay;
        }
        else
        {
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }

    public void SetPlayableCharacterVisible(bool visible)
    {
        characterSpawn.SetCharactersVisible(visible);
    }

    private void OnNewDay()
    {
        ++DaysInScene;
    }

    public bool HasInteractable(GameObject interactable)
    {
        if(interactables)
        {
            Transform parent = interactable.transform;
            while(parent && parent != interactables.transform)
            {
                parent = parent.transform.parent;
            }

            return parent && parent.gameObject == interactables;
        }

        return false;
    }
}
