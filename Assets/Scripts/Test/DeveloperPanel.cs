using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject locations;
    [SerializeField]
    private GameObject conditions;
    [SerializeField]
    private GameObject main;

    private void OnEnable()
    {
        locations.SetActive(false);
        conditions.SetActive(false);
        main.SetActive(true);
    }
}
