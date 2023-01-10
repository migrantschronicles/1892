using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Room : MonoBehaviour
{
    [SerializeField]
    private GameObject[] hiddenWhenVisited;
    [SerializeField]
    private GameObject[] shownWhenVisited;
    [SerializeField]
    private PlayableCharacterSpawn characterSpawn;
    [SerializeField]
    private LocalizedString roomName;

    private bool visited = false;

    public LocalizedString RoomName { get { return roomName; } }
    public RoomButton RoomButton { get; set; }
    public PlayableCharacterSpawn PlayableCharacterSpawn { get { return characterSpawn; } }

    private void Start()
    {
        if(characterSpawn == null)
        {
            Debug.LogError($"Character spawn is missing in {name}");
        }

        if(roomName == null || roomName.IsEmpty)
        {
            Debug.LogError($"Room name is missing in {name}");
        }

        if(!visited)
        {
            SetVisited(false);
        }

        LevelInstance.Instance.InstantiateRoomButton(this);
    }

    public void SetVisited(bool visited)
    {
        foreach(GameObject go in hiddenWhenVisited)
        {
            go.SetActive(!visited);
        }

        foreach(GameObject go in shownWhenVisited)
        {
            go.SetActive(visited);
        }

        if(visited)
        {
            characterSpawn.TrySpawn();
        }

        if(RoomButton)
        {
            RoomButton.SetActive(!visited);
        }

        this.visited = visited;
    }
}
