using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Information about the character when being displayed on the left or right in dialogs.
 */
public class CharacterDialogInfo : MonoBehaviour
{
    [SerializeField, Tooltip("The technical name in Articy of this character")]
    private string technicalName;
    [SerializeField, Tooltip("Whether the character looks to the left in Unity")]
    private bool looksLeft;
    [SerializeField, Tooltip("Scale in the dialogs. Useful for children if they should appear smaller.")]
    private float scaleFactor = 1.0f;

    public string TechnicalName { get { return technicalName; } }
    public bool LooksLeft { get { return looksLeft; } }
    public float ScaleFactor { get {  return scaleFactor; } }
    public GameObject Prefab { get { return gameObject; } }
}
