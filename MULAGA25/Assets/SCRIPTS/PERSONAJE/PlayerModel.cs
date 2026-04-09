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

    void Awake()
    {
        currentLives = maxLives;
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

        ConsejeroManager.Instance.EventoChoque();

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
        
        // Si tienes script de movimiento, desactívalo también
        // GetComponent<TuMovimiento>().enabled = false;

        MostrarUI();
    }

    void MostrarUI()
    {
        GameObject ui = Instantiate(deathCanvas);

        Transform cam = playerCamera;

        // 📍 Posición frente al jugador
        ui.transform.position = cam.position + cam.forward * 1.5f;

        // 🧭 Rotación hacia el jugador
        ui.transform.LookAt(cam);
        ui.transform.Rotate(0, 180, 0);
    }

    public void AddFragment()
    {
        posterFragments++;

        Debug.Log("Fragmentos recolectados: " + posterFragments);
    }
}