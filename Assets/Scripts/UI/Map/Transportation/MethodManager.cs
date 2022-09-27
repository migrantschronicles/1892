using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MethodManager : MonoBehaviour
{

    public GameObject gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");    
    }

    public void GoToLocation(string name) 
    {
        GetComponentInParent<TransportationButtons>().anim.SetBool("ButtonClicked", false);
        gameManager.GetComponent<NewGameManager>().GoToLocation(name, this.gameObject.name);
    }
}
