using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // 

public class VolumeController : MonoBehaviour
{
    public AudioMixer mixer;
    public string volumeParameter = "Volume";

    public Slider volumeSlider; // 

    private const string VOLUME_KEY = "GameVolume";

    void Start()
    {
        // Cargar volumen guardado
        float savedValue = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);

        SetVolume(savedValue);

        // 🔥 sincronizar el slider
        if (volumeSlider != null)
            volumeSlider.value = savedValue;
    }

    public void SetVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        float volume = Mathf.Log10(value) * 20;
        mixer.SetFloat(volumeParameter, volume);

        // Guardar valor
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
    }
}