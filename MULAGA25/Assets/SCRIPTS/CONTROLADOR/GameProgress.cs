using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("Tutorial")]
    public bool tutorialCompletado = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // Marcar tutorial como completado
    public void CompletarTutorial()
    {
        tutorialCompletado = true;
    }
}