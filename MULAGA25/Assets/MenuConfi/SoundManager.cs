using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public Slider VolumeSlider;
    void Start()
    {
        if (PlayerPrefs.HasKey("soundVolume"))
        {
            LoadVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("soundVolume", 1);
            LoadVolume();
        }
    }

    // Update is called once per frame
    public void Setvolume()
    {
        AudioListener.volume = VolumeSlider.value;
        SaveVolume();  
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("soundVolume", VolumeSlider.value);
    }

    public void LoadVolume()
    {
        if (PlayerPrefs.HasKey("soundVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("soundVolume");
            VolumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
        }
    }

    private static SoundManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
