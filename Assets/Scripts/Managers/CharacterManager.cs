using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private CharacterDialogInfo[] characters;

    public CharacterDialogInfo GetCharacterInfo(string technicalName)
    {
        CharacterDialogInfo character = characters.FirstOrDefault(character => character.TechnicalName == technicalName);
        if(!character)
        {
            Debug.LogError($"Could not find character info for: {technicalName}");
        }
        return character;
    }
    
    public GameObject GetCharacterPrefab(string technicalName)
    {
        CharacterDialogInfo character = GetCharacterInfo(technicalName);
        return character != null ? character.gameObject : null;
    }
}
