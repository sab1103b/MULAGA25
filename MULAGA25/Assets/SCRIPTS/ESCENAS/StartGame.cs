using UnityEngine;

public class StartGame : MonoBehaviour
{
    public FadeController fade;

    public string sceneToLoad;
    public string defaultScene = "Level_01";

    public void SetLevel(string sceneName)
    {
        sceneToLoad = sceneName;
        Debug.Log("Nivel seleccionado: " + sceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            string levelToLoad = string.IsNullOrEmpty(sceneToLoad)
                ? defaultScene
                : sceneToLoad;

            StartCoroutine(fade.FadeOut(levelToLoad));
        }
    }
}