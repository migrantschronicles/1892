using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource fxSource;
    [SerializeField]
    private AudioSource musicSource;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void PlayFX(AudioClip clip)
    {
        fxSource.Stop();
        fxSource.clip = clip;
        fxSource.Play();
    }
}
