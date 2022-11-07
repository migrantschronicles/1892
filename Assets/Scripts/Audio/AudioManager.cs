using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource fxSource;
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField]
    private AudioClip typewriterClip;
    [SerializeField]
    private float typewriterTime = 0.1f;
    [SerializeField]
    private AudioClip cutTypewriterClip;

    public static AudioManager Instance { get; private set; }

    private Coroutine typewriterCoroutine;
    private int typewriterCount = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayFX(AudioClip clip, bool stopRunning = true)
    {
        if((!stopRunning && fxSource.isPlaying) || !clip)
        {
            return; 
        }

        fxSource.Stop();
        fxSource.clip = clip;
        fxSource.Play();
    }

    public void StartTypewriter()
    {
        ++typewriterCount;
        if(typewriterCoroutine != null)
        {
            return;
        }

        typewriterCoroutine = StartCoroutine(PlayTypewriterCoroutine());
    }

    public void StopTypewriter()
    {
        if(typewriterCoroutine != null)
        {
            if(--typewriterCount == 0)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
        }
    }

    private IEnumerator PlayTypewriterCoroutine()
    {
        while(true)
        {
            PlayFX(typewriterClip, false);
            yield return new WaitForSeconds(typewriterTime);
        }
    }

    public void PlayCutTypewriter()
    {
        PlayFX(cutTypewriterClip);
    }
}
