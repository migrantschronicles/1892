using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextColorSwitch : MonoBehaviour
{
    public Text start;
    public Text quit;

    public void StartText() {
        start.color = Color.white;
    }
    public void QuitText() {
        quit.color = Color.white;
    }
}
