using UnityEngine;
using System.Collections;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    [Header("Tutorial")]
    public bool tutorialCompletado = false;

    [Header("Nivel 1")]
    public bool nivel1ObjetoRecogido = false;

    [Header("Progresión")]
    public int nivelActual = 0;

    private bool[] nivelesDesbloqueados = new bool[100];

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

    // ─────────────────────────────────────────────
    // TUTORIAL
    // ─────────────────────────────────────────────

    public void CompletarTutorial()
    {
        tutorialCompletado = true;

        UnlockLevel(1);

        nivelActual = 1;

        StartCoroutine(SecuenciaTutorial());
    }

    IEnumerator SecuenciaTutorial()
    {
        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento01();
        }

        yield return new WaitForSeconds(7f);

        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento02();
        }

        yield return new WaitForSeconds(12f);

        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento03();
        }

        yield return new WaitForSeconds(9f);

        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento04();
        }

        yield return new WaitForSeconds(7f);

        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento05();
        }
    }

    // ─────────────────────────────────────────────
    // PROGRESIÓN DE NIVELES
    // ─────────────────────────────────────────────

    public bool IsLevelUnlocked(int level)
    {
        return nivelesDesbloqueados[level];
    }

    public void UnlockLevel(int level)
    {
        nivelesDesbloqueados[level] = true;
    }

    public void CompletarNivel(int nivelCompletado)
    {
        int siguienteNivel = nivelCompletado + 1;

        UnlockLevel(siguienteNivel);

        nivelActual = siguienteNivel;
    }

    public void RecogerNivel1()
    {
        nivel1ObjetoRecogido = true;
    }
}