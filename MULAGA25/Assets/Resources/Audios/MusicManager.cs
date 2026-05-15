using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Clips")]
    public AudioClip defaultMusic;
    public AudioClip bossMusic;

    [Header("Settings")]
    public float fadeSpeed = 2f;

    private AudioSource audioSource;
    private float targetVolume = 1f;

    private bool bossMusicUnlocked = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        FindAudioSource();
    }

    void Start()
    {
        PlayDefault();
    }

    void Update()
    {
        if (audioSource == null) return;

        audioSource.volume = Mathf.Lerp(
            audioSource.volume,
            targetVolume,
            Time.deltaTime * fadeSpeed
        );
    }

    void FindAudioSource()
    {
        GameObject cameraOffset = GameObject.Find("Camera Offset");

        if (cameraOffset != null)
        {
            audioSource = cameraOffset.GetComponentInChildren<AudioSource>();

            if (audioSource == null)
                Debug.LogWarning("[MusicManager] No AudioSource en Camera Offset");
        }
        else
        {
            Debug.LogWarning("[MusicManager] No se encontró Camera Offset");
        }
    }

    public void PlayDefault()
    {
        if (audioSource == null) return;

        audioSource.clip = defaultMusic;
        audioSource.Play();
        targetVolume = 1f;
    }

    public void UnlockBossMusic()
    {
        if (bossMusicUnlocked) return;

        bossMusicUnlocked = true;

        if (audioSource == null) return;

        audioSource.clip = bossMusic;
        audioSource.Play();
        targetVolume = 1f;

        Debug.Log("🎵 Boss music activada permanentemente");
    }
}