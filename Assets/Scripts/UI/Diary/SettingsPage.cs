using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPage : MonoBehaviour
{
    [SerializeField]
    private AudioSlider musicSlider;
    [SerializeField]
    private AudioSlider sfxSlider;

    private bool isQuitting = false;

    private void Start()
    {
        float musicVolume = AudioManager.Instance.MusicVolume;
        musicSlider.Value = musicVolume;
        musicSlider.Slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        float sfxVolume = AudioManager.Instance.SFXVolume;
        sfxSlider.Value = sfxVolume;
        sfxSlider.Slider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnDisable()
    {
        SavePrefs();
    }

    private void OnApplicationFocus(bool focus)
    {
        if(!focus)
        {
            SavePrefs();
        }
    }

    private void SavePrefs()
    {
        if(isQuitting || !AudioManager.Instance.IsValid)
        {
            return;
        }

        PlayerPrefs.SetFloat("MusicVolume", AudioManager.Instance.MusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.SFXVolume);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnMusicVolumeChanged(float volume)
    {
        AudioManager.Instance.MusicVolume = volume;
    }

    private void OnSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SFXVolume = volume;
    }

    public void OnDownloadPDF()
    {
        NewGameManager.Instance.GeneratePDF();
    }

    public void OnBackToMainMenu()
    {
        LevelInstance.Instance.UI.IngameDiary.OnReturnToMainMenu();
    }
}
