using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomTransportationBehaviour : MonoBehaviour
{
    public Dropdown Origin;
    public Dropdown Destination;
    public InputField Type;
    public InputField Cost;
    public InputField Luggage;
    public InputField Speed;
    public InputField IconName;

    public Button AddButton;

    void Start()
    {
        AddButton.onClick.AddListener(AddCustomtransportation);
    }

    private void AddCustomtransportation()
    {
        var legKey = Origin.options[Origin.value].text + Destination.options[Destination.value].text;

        if(!CustomTransportationByLegKey.ContainsKey(legKey))
        {
            CustomTransportationByLegKey.Add(legKey, new List<CustomTransportation>());
        }

        CustomTransportationByLegKey[legKey].Add(new CustomTransportation()
        {
            Type = Type.text,
            Cost = int.Parse(Cost.text),
            Speed = int.Parse(Speed.text),
            IconName = IconName.text,
            Luggage = int.Parse(Luggage.text)
        });

        Destroy(gameObject);
    }

    public static Dictionary<string, List<CustomTransportation>> CustomTransportationByLegKey = new Dictionary<string, List<CustomTransportation>>();
}

public class CustomTransportation
{
    public string Type { get; set; }
    public int Cost { get; set; }
    public int Luggage { get; set; }
    public int Speed { get; set; }
    public string IconName { get; set; }
}
