using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Plays the specified audio clip if you call Play().
 * You can trigger this in button clicks.
 */
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
