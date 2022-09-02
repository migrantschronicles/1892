using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportationButtons : MonoBehaviour
{

    private Animator anim;

    public List<GameObject> transportationButtons;


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
        foreach (GameObject btn in transportationButtons) { 
            // Have to add check if this option is available.
            btn.SetActive(true);
        }
        anim.SetBool("ButtonClicked", true);
    }
}
