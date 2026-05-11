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

    private static SettingsMenu instance;

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

    void ToggleMenu()
    {
        isOpen = !isOpen;

        if (panel != null)
            panel.SetActive(isOpen);

        if (isOpen)
        {
            // 🔥 PAUSAR
            Time.timeScale = 0f;

            bool isLobby = SceneManager.GetActiveScene().name == "LobbyScene";

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
        Time.timeScale = 1f;

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;    
    #else
            Application.Quit();
    #endif
        }

    public void VolverLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }

    public void VolverNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}