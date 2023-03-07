using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField]
    private float musicFadeTime = 1.0f;

    public static AudioManager Instance { get; private set; }

    private Coroutine typewriterCoroutine;
    private int typewriterCount = 0;
    private List<AudioClip> currentMusicClips;
    private Coroutine fadeMusicCoroutine;
    private Coroutine playMusicCoroutine;

    public float MusicVolume 
    { 
        get
        {
            MusicMixer.GetFloat("MusicVolume", out float currentVolume);
            float normalized = ConvertMixerToNormalized(currentVolume);
            return Mathf.Abs(normalized) < 0.0005f ? 0.0f : normalized;
        }
        set
        {
            float mixerVolume = ConvertNormalizedToMixer(value);
            MusicMixer.SetFloat("MusicVolume", mixerVolume);
        }
    }

    public float SFXVolume
    {
        get
        {
            SFXMixer.GetFloat("FXVolume", out float currentVolume);
            float normalized = ConvertMixerToNormalized(currentVolume);
            return Mathf.Abs(normalized) < 0.0005f ? 0.0f : normalized;
        }
        set
        {
            float mixerVolume = ConvertNormalizedToMixer(value);
            SFXMixer.SetFloat("FXVolume", mixerVolume);
        }
    }

    private AudioMixer MusicMixer
    {
        get
        {
            return musicSource.outputAudioMixerGroup.audioMixer;
        }
    }

    private AudioMixer SFXMixer
    {
        get
        {
            return fxSource.outputAudioMixerGroup.audioMixer;
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            transform.SetParent(null, false);
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

    public void PlayMusic(IEnumerable<AudioClip> clips)
    {
        if(fadeMusicCoroutine != null)
        {
            StopCoroutine(fadeMusicCoroutine);
            fadeMusicCoroutine = null;
        }

        if(playMusicCoroutine != null)
        {
            StopCoroutine(playMusicCoroutine);
            playMusicCoroutine = null;
        }

        SetMusicVolume(1.0f);

        currentMusicClips = new List<AudioClip>(clips);
        if(currentMusicClips != null && currentMusicClips.Count > 0)
        {
            playMusicCoroutine = StartCoroutine(PlayMusicRandomly());
        }
    }

    private IEnumerator PlayMusicRandomly()
    {
        while(true)
        {
            AudioClip nextClip;
            int tries = 0;
            do
            {
                nextClip = currentMusicClips[((int)(Random.value * currentMusicClips.Count)) % currentMusicClips.Count];
                ++tries;
            }
            while (currentMusicClips.Count > 1 && nextClip == musicSource.clip && tries < 5);
            musicSource.clip = nextClip;
            musicSource.Play();
            yield return new WaitWhile(() => musicSource.isPlaying);
        }
    }

    public void FadeOutMusic()
    {
        if(musicSource.isPlaying && fadeMusicCoroutine == null)
        {
            fadeMusicCoroutine = StartCoroutine(StartMusicFadeOut());
        }
    }

    private IEnumerator StartMusicFadeOut()
    {
        yield return StartMusicFade(0.0f);
        musicSource.Stop();

        if(playMusicCoroutine != null)
        {
            StopCoroutine(playMusicCoroutine);
            playMusicCoroutine = null;
        }
    }

    private IEnumerator StartMusicFade(float targetVolume)
    {
        float currentTime = 0;
        float currentVolume = MusicVolume;
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while(currentTime < musicFadeTime)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / musicFadeTime);
            SetMusicVolume(newVolume);
            yield return null;
        }

        fadeMusicCoroutine = null;
    }

    private void SetMusicVolume(float volume)
    {
        MusicMixer.SetFloat("MusicVolume", ConvertNormalizedToMixer(volume));
    }

    private float ConvertNormalizedToMixer(float normalized)
    {
        return Mathf.Log10(Mathf.Clamp(normalized, 0.0001f, 1)) * 20;
    }

    private float ConvertMixerToNormalized(float mixer)
    {
        return Mathf.Pow(10, mixer / 20);
    }
}
