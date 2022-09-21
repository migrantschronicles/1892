using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportationButtons : MonoBehaviour
{

    public bool capital = false;
    public bool available = false;
    public Animator anim;

    public List<GameObject> availableRoutes;

    [Space(20)]
    public GameObject transportationMethodsGO;
    public List<GameObject> transportationButtons;

    public bool walking, tram, carriage, cart, ship, train;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableTransportationOptions() 
    {
        transportationMethodsGO.SetActive(true);
        // Turning on available methods of transportation
        foreach (GameObject btn in transportationButtons)
        {
            if (btn.name == "Walking" && walking)
            {
                btn.SetActive(true);
            }
            else if (btn.name == "Tram" && tram)
            {
                btn.SetActive(tram);
            }
            else if (btn.name == "Carriage" && carriage)
            {
                btn.SetActive(true);
            }
            else if (btn.name == "Cart" && cart)
            {
                btn.SetActive(true);
            }
            else if (btn.name == "Ship" && ship)
            {
                btn.SetActive(true);
            }
            else if (btn.name == "Train" && train)
            {
                btn.SetActive(true);
            }
        }

        anim.SetBool("ButtonClicked", true);        
    }

    public void DisableTransportationOptions() 
    {
        anim.SetBool("ButtonClicked", false);
        transportationMethodsGO.SetActive(false);
    }

}
