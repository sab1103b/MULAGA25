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

    void Awake()
    {
        currentLives = maxLives;

        BuscarReferencias();
    }

    void BuscarReferencias()
    {
        // -----------------------------------
        // MAIN CAMERA
        // -----------------------------------
        Camera cam = Camera.main;

        if (cam != null)
        {
            playerCamera = cam.transform;
        }

        // -----------------------------------
        // HUD_VR
        // -----------------------------------
        GameObject hudVR = GameObject.Find("HUD_VR");

        if (hudVR != null)
        {
            hud = hudVR.GetComponentInChildren<HUD_HealthSystem>(true);
        }

        // -----------------------------------
        // UI DEATH CANVAS
        // -----------------------------------
        GameObject deathUI = GameObject.Find("UI Death Canvas");

        if (deathUI != null)
        {
            deathCanvas = deathUI;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives -= amount;

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

        // POSICIÓN FRENTE AL JUGADOR
        ui.transform.position =
            playerCamera.position +
            playerCamera.forward * 1.5f;

        // ROTACIÓN HACIA EL JUGADOR
        ui.transform.LookAt(playerCamera);
        ui.transform.Rotate(0, 180, 0);
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