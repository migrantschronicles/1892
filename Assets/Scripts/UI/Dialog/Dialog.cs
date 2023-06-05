using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DialogProtagonistMode
{
    /// Every protagonist is displayed on the right (e.g. family: Elis, Matti, Mreis).
    Any,
    /// Only the main protagonist is displayed on the right (e.g. family: Elis. Matti and Mreis are on the left).
    OnlyMain
}

public class Dialog : MonoBehaviour
{
    [Tooltip("This dialog only plays if this condition is met")]
    public DialogCondition Condition;
    [Tooltip("The condition that is set when the dialog should restart.")]
    public string restartCondition;
    [Tooltip("The conditions that should be set when the dialog is fininished")]
    public SetCondition[] setFinishedConditions;
    [Tooltip("Where to display the protagonists")]
    public DialogProtagonistMode protagonistMode;

    public ArticyObject ArticyObject { get { return GetComponent<ArticyReference>().reference.GetObject(); } }
}
