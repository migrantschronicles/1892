using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCharacterManager : MonoBehaviour
{
    [System.Serializable]
    class PlayerCharacterEntry
    {
        public CharacterType Character;
        public PlayableCharacterData Data;
    }

    [SerializeField]
    private PlayerCharacterEntry[] characters;

    private CharacterType selectedCharacter = CharacterType.None;

    private PlayerCharacterEntry SelectedEntry { get => characters.First(character => character.Character == selectedCharacter); }
    public CharacterType SelectedCharacter { get => selectedCharacter; }
    public PlayableCharacterData SelectedData { get => SelectedEntry.Data; }

    public void Initialize(CharacterType character = CharacterType.None)
    {
        if(character == CharacterType.None)
        {
            character = CharacterType.Elis;
        }

        selectedCharacter = character;
        NewGameManager.Instance.conditions.InitCharacter(selectedCharacter);
    }
}