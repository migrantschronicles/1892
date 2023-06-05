using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDialogInfo : MonoBehaviour
{
    [SerializeField]
    private string technicalName;
    [SerializeField, Tooltip("Whether the character looks to the left")]
    private bool looksLeft;
    [SerializeField]
    private float scaleFactor = 1.0f;

    public string TechnicalName { get { return technicalName; } }
    public bool LooksLeft { get { return looksLeft; } }
    public float ScaleFactor { get {  return scaleFactor; } }
    public GameObject Prefab { get { return gameObject; } }
}
