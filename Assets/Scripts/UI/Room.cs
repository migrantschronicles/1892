using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

/**
 * Represents a room in the ship.
 * If the room has any interactables, add a SceneInteractables object to the SceneInteractables of the Canvas,
 * and add your buttons there. Then add this new SceneInteractables object to shownWhenVisited.
 * Don't use the normal buttons, but the ones with _Ship ending (e.g. DialogButton_Ship).
 * The room button is automatically created with the roomName.
 * Each room has a character spawn to spawn the playable character to.
 */
public class Room : MonoBehaviour
{
    [SerializeField]
    private GameObject interactables;
    [SerializeField]
    private GameObject[] hiddenWhenVisited;
    [SerializeField]
    private GameObject[] shownWhenVisited;
    [SerializeField]
    private PlayableCharacterSpawn characterSpawn;
    [SerializeField]
    private GameObject middleground;
    [SerializeField]
    private LocalizedString roomName;
    [SerializeField]
    private Vector3 roomButtonWorldObjectOffset = new Vector3(0, 0);
    [SerializeField]
    private SpriteRenderer overlay;
    [SerializeField]
    private bool steerageClassAccessible = true;
    [SerializeField]
    private bool secondClassAccessible = true;
    [SerializeField]
    private bool firstClassAccessible = true;
    [SerializeField]
    private Color inaccessibleColor = new Color(0, 0, 0, 0.9f);
    [SerializeField]
    private Color unvisitedColor = new Color(0, 0, 0, 0.5f);

    private bool visited = false;
    private bool accessible = true;

    public LocalizedString RoomName { get { return roomName; } }
    public RoomButton RoomButton { get; set; }
    public PlayableCharacterSpawn PlayableCharacterSpawn { get { return characterSpawn; } }
    public Vector3 RoomButtonWorldObjectOffset { get { return roomButtonWorldObjectOffset; } }
    public bool IsAccessible { get {  return accessible; } }
    public GameObject Middleground { get { return middleground; } }
    public GameObject Interactables { get {  return interactables; } }

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

        // If you start play in the ship in the editor, make rooms accessible by default.
        bool isAccessible =
#if UNITY_EDITOR
            NewGameManager.Instance.ShipManager.IsTravellingInShip ? false : true;
#else
            false;
#endif
        switch(NewGameManager.Instance.ShipManager.ShipClass)
        {
            case ShipClass.First: isAccessible = firstClassAccessible; break;
            case ShipClass.Second: isAccessible = secondClassAccessible; break;
            case ShipClass.Steerage: isAccessible = steerageClassAccessible; break;
        }

        SetIsAccessible(isAccessible);
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

        interactables.SetActive(visited);

        if(visited)
        {
            characterSpawn.TrySpawn();
        }
        else
        {
            characterSpawn.DestroyCharacters();
        }

        if(RoomButton)
        {
            RoomButton.SetActive(!visited);
        }

        this.visited = visited;
        UpdateOverlay();
    }

    private void SetIsAccessible(bool isAccessible)
    {
        accessible = isAccessible;
        UpdateOverlay();
    }

    private void UpdateOverlay()
    {
        if(visited)
        {
            overlay.gameObject.SetActive(false);
        }
        else
        {
            overlay.gameObject.SetActive(true);
            overlay.color = accessible ? unvisitedColor : inaccessibleColor;
            overlay.GetComponent<Collider2D>().enabled = accessible;
        }
    }
}
