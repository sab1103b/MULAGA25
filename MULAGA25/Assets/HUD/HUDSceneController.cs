using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDSceneController : MonoBehaviour
{
    [Header("Componentes del HUD")]
    public GameObject vidas;
    public GameObject consejeroContainer;
    public GameObject hudContadores;

    [Header("Detector del arma en Lobby")]
    public AreaDetector areaDetector;

    private static HUDSceneController instance;

    void Awake()
    {
        // Evitar duplicados
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Mantener entre escenas
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ActualizarHUD();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ActualizarHUD();
    }

    void Update()
    {
        // Actualización dinámica en Lobby
        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            if (areaDetector != null)
            {
                hudContadores.SetActive(!areaDetector.objetoDentro);
            }
        }
    }

    void ActualizarHUD()
    {
        string escenaActual = SceneManager.GetActiveScene().name;

        // -------------------------
        // LEVEL 01
        // -------------------------
        if (escenaActual == "Level_01")
        {
            vidas.SetActive(true);
            hudContadores.SetActive(true);
        }

        else if (escenaActual == "Level_02")
        {
            vidas.SetActive(true);
            hudContadores.SetActive(true);
        }
        // -------------------------
        // LOBBY
        // -------------------------
        else if (escenaActual == "LobbyScene")
        {
            vidas.SetActive(false);

            if (areaDetector != null && areaDetector.objetoDentro)
                hudContadores.SetActive(false);
            else
                hudContadores.SetActive(true);
        }

        // -------------------------
        // OTRAS ESCENAS
        // -------------------------
        else
        {
            vidas.SetActive(false);
            consejeroContainer.SetActive(false);
            hudContadores.SetActive(false);
        }
    }
}