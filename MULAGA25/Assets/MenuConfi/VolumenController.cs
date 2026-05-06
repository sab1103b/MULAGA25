using UnityEngine;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public AudioMixer mixer;
    public string volumeParameter = "Volume";

    public void SetVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        float volume = Mathf.Log10(value) * 20;
        mixer.SetFloat(volumeParameter, volume);
    }
}
