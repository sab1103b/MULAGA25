using UnityEngine;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public AudioMixer mixer;
    public string volumeParameter = "Volume";

    public void SetVolume(float value)
    {
        // Convertir a decibeles
        float volume = Mathf.Log10(value) * 20;

        mixer.SetFloat(volumeParameter, volume);
    }
}
