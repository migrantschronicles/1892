using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadButtonCheck : MonoBehaviour
{

    public SaveGameManager sgm;

    public void OnEnable()
    {
        if (sgm.SavedGameExists && !sgm.DataFile.hasFinishedGame) 
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
