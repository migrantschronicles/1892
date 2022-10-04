using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MethodManager : MonoBehaviour
{
    public float time = 0;
    public int money = 0;
    public int food = 0;

    void OnEnable() 
    {
        transform.GetChild(0).GetComponent<TransportationMethodInfo>().time.text = time.ToString();
        transform.GetChild(0).GetComponent<TransportationMethodInfo>().money.text = money.ToString();
        transform.GetChild(0).GetComponent<TransportationMethodInfo>().food.text = food.ToString();
    }

    public void GoToLocation(string name) // Add resources input 
    {
        GetComponentInParent<TransportationButtons>().anim.SetBool("ButtonClicked", false);
        NewGameManager.Instance.GoToLocation(name, this.gameObject.name, time, money, food);
    }
}
