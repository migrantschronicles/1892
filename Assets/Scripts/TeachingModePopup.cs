using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachingModePopup : MonoBehaviour
{
    public GameObject popupContainer;
    public UnityEngine.UI.Button teachingModeButton;
    public UnityEngine.UI.Button closeDialogueButton;
    public UnityEngine.UI.Text text;
    public Color color;

    //public delegate void OnClickDelegate();
    //public event OnClickDelegate onClick;

    private void Start()
    {
        teachingModeButton.onClick.AddListener(ShowPopup);
        closeDialogueButton.onClick.AddListener(HidePopup);
    }

    public void ShowPopup()
    {
        text.color = new Color(255, 255, 255);
        popupContainer.SetActive(true);
    }

    public void HidePopup()
    {
        text.color = color;
        popupContainer.SetActive(false);
    }

}
