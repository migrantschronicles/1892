using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private GameObject fillArea;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Color offColor;

    public Slider Slider { get { return slider; } }

    public float Value
    {
        get
        {
            return slider.value;
        }
        set
        {
            slider.value = value;
        }
    }

    private void Awake()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        bool isOff = Mathf.Approximately(value, 0.0f);
        fillArea.SetActive(!isOff);
        background.color = isOff ? offColor : Color.white;
    }
}
