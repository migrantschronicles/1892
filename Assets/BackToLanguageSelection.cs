using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToLanguageSelection : MonoBehaviour
{
    public void BackToSelection()
    {
        Debug.Log("BackToSelection Activated");
        GetComponentInParent<Animator>().SetTrigger("Close");
        this.transform.parent.transform.parent.GetComponentInParent<MainMenu>().OpenLanguageSelection();
        
    }
}
