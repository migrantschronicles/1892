using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFX : MonoBehaviour
{
    [SerializeField]
    private AudioClip fxClip;

    public void Play()
    {
        AudioManager.Instance.PlayFX(fxClip);
    }

    public void Play(AudioClip clip)
    {
        AudioManager.Instance.PlayFX(clip);
    }
}
