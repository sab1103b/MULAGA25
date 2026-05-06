using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mixer;

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        float db = Mathf.Log10(value) * 20;
        mixer.SetFloat("MusicVolume", db);
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        float db = Mathf.Log10(value) * 20;
        mixer.SetFloat("SFXVolume", db);
    }
}