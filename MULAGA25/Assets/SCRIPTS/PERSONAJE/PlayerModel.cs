using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public HUD_HealthSystem hud;

    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;
    public bool isDead = false;

    [Header("Collectibles")]
    public int posterFragments = 0;

    [Header("Death UI")]
    public GameObject deathCanvas;
    public Transform playerCamera;

    [Header("Throwables")]
    public int maxGrenades = 3;
    public int currentGrenades = 0;

    public int maxShields = 1;
    public int currentShields = 0;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip footstepSound;

    void Awake()
    {
        currentLives = maxLives;

        BuscarReferencias();
    }

    void BuscarReferencias()
    {
        // MAIN CAMERA
        Camera cam = Camera.main;

        if (cam != null)
        {
            playerCamera = cam.transform;

            // AUDIO SOURCE DE LA CAMARA
            if (audioSource == null)
            {
                audioSource = cam.GetComponent<AudioSource>();

                if (audioSource == null)
                {
                    audioSource = cam.GetComponentInChildren<AudioSource>();
                }
            }
        }

        // HUD_VR
        GameObject hudVR = GameObject.Find("HUD_VR");

        if (hudVR != null)
        {
            hud = hudVR.GetComponentInChildren<HUD_HealthSystem>(true);
        }

        // UI DEATH CANVAS
        GameObject deathUI = GameObject.Find("UI Death Canvas");

        if (deathUI != null)
        {
            deathCanvas = deathUI;
        }

        // CARGA DE AUDIOS (RESOURCES)
        // -----------------------------------
        hitSound = Resources.Load<AudioClip>("Audios/Efectos/Jugador/Hit");
        deathSound = Resources.Load<AudioClip>("Audios/Efectos/Jugador/Derrota");
        footstepSound = Resources.Load<AudioClip>("Audios/Efectos/Jugador/Pasos");

    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives -= amount;

        // SONIDO DE DAÑO
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (currentLives <= 0)
        {
            currentLives = 0;
            Morir();
        }

        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento09();
        }

        Debug.Log("Vidas restantes: " + currentLives);

        // ACTUALIZAR HUD
        if (hud != null)
        {
            hud.SetHealth(currentLives);
        }
    }

    void Morir()
    {
        isDead = true;

        // SONIDO DE MUERTE
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Time.timeScale = 0f;

        // BLOQUEAR MOVIMIENTO
        var controller = GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        MostrarUI();
    }

    void MostrarUI()
    {
        if (deathCanvas == null || playerCamera == null)
            return;

        GameObject ui = Instantiate(deathCanvas);

        // Dirección frente al jugador (plano horizontal)
        Vector3 forward = playerCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        // Posición a 2 unidades
        Vector3 pos = playerCamera.position + forward * 2f;
        ui.transform.position = pos;

        // ROTACIÓN SOLO EN Y
        Vector3 lookDirection = playerCamera.position - ui.transform.position;
        lookDirection.y = 0f;

        ui.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    // -------------------------
    // FOOTSTEPS
    // -------------------------
    public void PlayFootstep()
    {
        if (footstepSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(footstepSound);
        }
    }

    public void AddFragment()
    {
        posterFragments++;

        Debug.Log("Fragmentos recolectados: " + posterFragments);
    }

    public void AddGrenade(int amount = 1)
    {
        currentGrenades =
            Mathf.Clamp(currentGrenades + amount, 0, maxGrenades);
    }

    public void UseGrenade(int amount = 1)
    {
        currentGrenades =
            Mathf.Clamp(currentGrenades - amount, 0, maxGrenades);
    }

    public void AddShield(int amount = 1)
    {
        currentShields =
            Mathf.Clamp(currentShields + amount, 0, maxShields);
    }

    public void UseShield(int amount = 1)
    {
        currentShields =
            Mathf.Clamp(currentShields - amount, 0, maxShields);
    }
}