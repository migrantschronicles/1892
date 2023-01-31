using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialog : MonoBehaviour
{
    [Tooltip("This dialog only plays if this condition is met")]
    public DialogCondition Condition;

    public ArticyObject ArticyObject { get { return GetComponent<ArticyReference>().reference.GetObject(); } }
}
