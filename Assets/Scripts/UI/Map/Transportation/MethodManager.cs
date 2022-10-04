using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MethodManager : MonoBehaviour
{

    public GameObject gameManager;
    public float time = 0;
    public float money = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");    
    }

    public void GoToLocation(string name) // Add resources input 
    {
        GetComponentInParent<TransportationButtons>().anim.SetBool("ButtonClicked", false);
        gameManager.GetComponent<NewGameManager>().GoToLocation(name, this.gameObject.name);
    }
}
