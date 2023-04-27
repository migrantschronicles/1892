using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadButtonCheck : MonoBehaviour
{

    public SaveGameManager sgm;

    public void OnEnable()
    {
        if (sgm.SavedGameExists) 
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
