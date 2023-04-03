using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLocationCityText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(this.GetComponent<Text>().text == "Location") 
        {
            this.GetComponent<Text>().text = NewGameManager.Instance.LocationManager.GetLocalizedName(this.transform.parent.name);
        }
    }
}
