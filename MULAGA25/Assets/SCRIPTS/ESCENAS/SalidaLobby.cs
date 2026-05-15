using UnityEngine;

public class SalidaLobby : MonoBehaviour
{
    public FadeController fade;
    public string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            StartCoroutine(fade.FadeOut(sceneToLoad));
        }
    }
}