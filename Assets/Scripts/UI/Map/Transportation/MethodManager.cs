using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MethodManager : MonoBehaviour
{
    public void GoToLocation(string name) 
    {
        GetComponentInParent<TransportationButtons>().anim.SetBool("ButtonClicked", false);
        NewGameManager.Instance.GoToLocation(name, this.gameObject.name);
    }
}
