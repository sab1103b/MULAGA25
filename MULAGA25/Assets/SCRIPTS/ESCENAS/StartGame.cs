using UnityEngine;

public class StartGame : MonoBehaviour
{
    public FadeController fade;
    public string sceneToLoad;

    public void SetLevel(string sceneName)
    {
        sceneToLoad = sceneName;
        Debug.Log("Nivel seleccionado: " + sceneToLoad);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(fade.FadeOut(sceneToLoad));
        }
    }
}