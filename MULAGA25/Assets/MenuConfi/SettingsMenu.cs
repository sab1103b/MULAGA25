using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public GameObject lobbyButtons;
    public GameObject gameButtons;

    [Header("Input VR")]
    public InputActionProperty openMenuButton;

    private bool isOpen = false;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void OnEnable()
    {
        if (openMenuButton.action != null)
            openMenuButton.action.Enable();
    }

    void OnDisable()
    {
        if (openMenuButton.action != null)
            openMenuButton.action.Disable();
    }

    void Update()
    {
        if (openMenuButton.action != null && openMenuButton.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isOpen = !isOpen;

        if (panel != null)
            panel.SetActive(isOpen);

        if (isOpen)
        {
            // 🔥 PAUSAR
            Time.timeScale = 0f;

            bool isLobby = SceneManager.GetActiveScene().name == "Lobby";

            if (lobbyButtons != null)
                lobbyButtons.SetActive(isLobby);

            if (gameButtons != null)
                gameButtons.SetActive(!isLobby);

            // 📍 Posicionar menú frente al jugador
            VRMenuPositioner pos = GetComponent<VRMenuPositioner>();
            if (pos != null)
                pos.PlaceMenu();
        }
        else
        {
            // 🔥 REANUDAR
            Time.timeScale = 1f;
        }
    }

    // =========================
    // BOTONES UI
    // =========================

    public void VolverJuego()
    {
        isOpen = false;

        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void SalirJuego()
    {
        Application.Quit();
    }

    public void VolverLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Lobby");
    }

    public void VolverNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}