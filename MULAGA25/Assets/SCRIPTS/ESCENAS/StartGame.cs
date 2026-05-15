using UnityEngine;

public class StartGame : MonoBehaviour
{
    public FadeController fade;

    public string sceneToLoad;
    public string defaultScene = "Level_01";

    [Header("Nivel por defecto")]
    public int defaultLevelNumber = 1;

    private int selectedLevelNumber = 0;

    public void SetLevel(string sceneName, int levelNumber)
    {
        sceneToLoad = sceneName;
        selectedLevelNumber = levelNumber;

        Debug.Log("Nivel seleccionado: " + sceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            // NO se seleccionó nivel
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                if (GameProgress.Instance.IsLevelUnlocked(defaultLevelNumber))
                {
                    StartCoroutine(fade.FadeOut(defaultScene));
                }
                else
                {
                    Debug.Log("Nivel bloqueado. Completa el tutorial.");
                }

                return;
            }

            // Se seleccionó nivel → validar desbloqueo
            if (GameProgress.Instance.IsLevelUnlocked(selectedLevelNumber))
            {
                StartCoroutine(fade.FadeOut(sceneToLoad));
            }
            else
            {
                Debug.Log("Nivel bloqueado");
            }
        }
    }
}