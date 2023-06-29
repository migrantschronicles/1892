using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraAnimReceiver : MonoBehaviour
{
    public void OnSetDiaryToClosed()
    {
        MainMenuController.Instance.OnSetDiaryToClosed();
    }
}
