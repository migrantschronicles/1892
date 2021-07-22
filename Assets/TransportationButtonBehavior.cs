using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransportationButtonBehavior : MonoBehaviour
{
    public Text TransportationType;
    public Text LuggageSpace;
    public Text Duration;
    public Text Cost;

    void Start()
    {
        //GetComponent<Button>().onClick.AddListener(Travel);
    }

    private void Travel()
    {
        //throw new NotImplementedException();
    }

    void Update()
    {
        
    }
}
