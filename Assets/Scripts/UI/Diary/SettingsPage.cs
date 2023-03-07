using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPage : MonoBehaviour
{
    [SerializeField]
    private AudioSlider musicSlider;
    [SerializeField]
    private AudioSlider sfxSlider;

    private void Start()
    {
        float musicVolume = AudioManager.Instance.MusicVolume;
        musicSlider.Value = musicVolume;
        musicSlider.Slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        float sfxVolume = AudioManager.Instance.SFXVolume;
        sfxSlider.Value = sfxVolume;
        sfxSlider.Slider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMusicVolumeChanged(float volume)
    {
        AudioManager.Instance.MusicVolume = volume;
    }

    private void OnSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SFXVolume = volume;
    }
}
