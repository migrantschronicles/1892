using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationExample : MonoBehaviour
{
    public void Vibrate()
    {
        Debug.Log(Application.isMobilePlatform);
        NewGameManager.Vibrate();
    }
}
