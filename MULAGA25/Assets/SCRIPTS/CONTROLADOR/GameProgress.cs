using UnityEngine;
using System.Collections;

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

        StartCoroutine(SecuenciaTutorial());
    }

    IEnumerator SecuenciaTutorial()
    {
        if (ConsejeroManager.Instance != null)
        {
            ConsejeroManager.Instance.Evento01();
        }

        // Esperar 15 segundos
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
}